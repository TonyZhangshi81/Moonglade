﻿using Moonglade.Caching;
using Moonglade.Configuration;
using Moonglade.Core;
using Moonglade.Web.Pages;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;

namespace Moonglade.Web.Tests.Pages
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class TagListModelTests
    {
        private MockRepository _mockRepository;

        private Mock<ITagService> _mockTagService;
        private Mock<IBlogConfig> _mockBlogConfig;
        private Mock<IPostQueryService> _mockPostQueryService;
        private Mock<IBlogCache> _mockBlogCache;

        private readonly IReadOnlyList<PostDigest> _fakePosts = new List<PostDigest>
        {
            new()
            {
                Title = "“996”工作制，即每天早 9 点到岗，一直工作到晚上 9 点，每周工作 6 天。",
                ContentAbstract = "中国大陆工时规管现况（标准工时）： 一天工作时间为 8 小时，平均每周工时不超过 40 小时；加班上限为一天 3 小时及一个月 36 小时，逾时工作薪金不低于平日工资的 150%。而一周最高工时则为 48 小时。平均每月计薪天数为 21.75 天。",
                LangCode = "zh-CN",
                PubDateUtc = new(996, 9, 6),
                Slug = "996-icu",
                Tags = new Tag[]{
                    new ()
                    {
                        DisplayName = "996",
                        Id = 996,
                        NormalizedName = "icu"
                    }
                }
            }
        };

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new(MockBehavior.Default);

            _mockTagService = _mockRepository.Create<ITagService>();
            _mockBlogConfig = _mockRepository.Create<IBlogConfig>();
            _mockPostQueryService = _mockRepository.Create<IPostQueryService>();
            _mockBlogCache = _mockRepository.Create<IBlogCache>();

            _mockBlogConfig.Setup(p => p.ContentSettings).Returns(new ContentSettings
            {
                PostListPageSize = 10
            });
        }

        private TagListModel CreateTagListModel()
        {
            return new(
                _mockTagService.Object,
                _mockBlogConfig.Object,
                _mockPostQueryService.Object,
                _mockBlogCache.Object);
        }

        [Test]
        public async Task OnGet_NullTag()
        {
            _mockTagService.Setup(p => p.Get(It.IsAny<string>())).Returns((Tag)null);

            // Arrange
            var tagListModel = CreateTagListModel();
            string normalizedName = "996";

            // Act
            var result = await tagListModel.OnGet(normalizedName);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task OnGet_ValidTag()
        {
            _mockTagService.Setup(p => p.Get(It.IsAny<string>())).Returns(new Tag
            {
                Id = 996,
                DisplayName = "Fubao",
                NormalizedName = "fu-bao"
            });

            _mockPostQueryService.Setup(p => p.ListByTag(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(_fakePosts));

            _mockBlogCache.Setup(p =>
                    p.GetOrCreate(CacheDivision.PostCountTag, It.IsAny<string>(), It.IsAny<Func<ICacheEntry, int>>()))
                .Returns(251);

            var httpContext = new DefaultHttpContext();
            var modelState = new ModelStateDictionary();
            var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            var pageContext = new PageContext(actionContext)
            {
                ViewData = viewData
            };

            var model = new TagListModel(
                _mockTagService.Object,
                _mockBlogConfig.Object,
                _mockPostQueryService.Object,
                _mockBlogCache.Object)
            {
                PageContext = pageContext,
                TempData = tempData
            };

            string normalizedName = "fu-bao";

            // Act
            var result = await model.OnGet(normalizedName);
            Assert.IsInstanceOf<PageResult>(result);
            Assert.AreEqual(251, model.Posts.TotalItemCount);
        }
    }
}
