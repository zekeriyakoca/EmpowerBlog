﻿using EmpowerBlog.Services.Post.API.Infrastructure;
using System.Threading.Tasks;

namespace EmpowerBlog.Services.Post.infrastructure.CQRS
{
    public interface ICommandHandler<TCommand, TResult>
    {
        public Task<TResult> Execute(TCommand query);
    }

    public abstract class CommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    {
        public CommandHandler()
        {
        }

        public async Task<TResult> Execute(TCommand command)
        {
            var result = await Action(command);

            return result;
        }

        public abstract Task<TResult> Action(TCommand query);



    }
}
