﻿using MediatR;
using Moonglade.Data.Entities;
using Moonglade.Data.Infrastructure;
using Moonglade.Data.Spec;

namespace Moonglade.Core.PostFeature
{
    public class GetPostByIdQuery : IRequest<Post>
    {
        public GetPostByIdQuery(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }

    public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, Post>
    {
        private readonly IRepository<PostEntity> _postRepo;

        public GetPostByIdQueryHandler(IRepository<PostEntity> postRepo)
        {
            _postRepo = postRepo;
        }

        public Task<Post> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
        {
            var spec = new PostSpec(request.Id);
            var post = _postRepo.SelectFirstOrDefaultAsync(spec, Post.EntitySelector);
            return post;
        }
    }
}
