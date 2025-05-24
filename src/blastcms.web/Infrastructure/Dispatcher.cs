using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Microsoft.CodeAnalysis.Differencing;

namespace blastcms.web.Infrastructure
{

    public interface IRequest { }
    public interface IRequest<TResult> { }

    public interface IDispatcher
    {

        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
        Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : class, IRequest;

    }

    public interface IRequestHandler<in TRequest, TResponse> where TRequest : class, IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    public interface IRequestHandler<in TRequest>   where TRequest : class, IRequest
    {
        Task Handle(TRequest request, CancellationToken cancellationToken);
    }


    public class Dispatcher(IServiceProvider _provider) : IDispatcher
    {

        public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : class, IRequest
        {
            var handler = _provider.GetService<IRequestHandler<TRequest>>()
                ?? throw new InvalidOperationException($"No handler for {typeof(TRequest).Name}");
            await handler.Handle(request, cancellationToken);
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken) 
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            dynamic handler = _provider.GetService(handlerType)
                ?? throw new InvalidOperationException($"No handler for {request.GetType().Name}");
            var methodInfo = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));
            return await (Task<TResponse>) methodInfo.Invoke(handler, new object[] { request, cancellationToken });
        }
    }
}
