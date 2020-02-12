using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SchoolRpc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Service.EventFeed.gRpcTfgRedis.Interceptors
{
    public class LogAndExceptionInterceptor : Interceptor
    {
        private readonly ILogger _logger;
        DateTime requestTime = DateTime.UtcNow;

        DateTime responseTime = default(DateTime);

        public LogAndExceptionInterceptor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("");
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var response = default(TResponse);
            try
            {
                response = await base.UnaryServerHandler(request, context, continuation);
                LogInformation(context.Method, request, response);
            }
            catch (Exception e)
            {
                return LogExceptionAndReturnResponse(context.Method, request, response, e);
            }
            return response;
        }

        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
           IAsyncStreamReader<TRequest> requestStream,
           ServerCallContext context,
           ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            var requestString = default(string);
            context.GetHttpContext().Request.EnableBuffering();
            var response = default(TResponse);

            try
            {
                var sr = new StreamReader(context.GetHttpContext().Request.Body);
                requestString = await sr.ReadToEndAsync();//目前讀出來會是亂碼，還找不到方式解
                context.GetHttpContext().Request.Body.Position = 0;//因有使用了EnableBuffering()此extention所以才能操作position

                response = await base.ClientStreamingServerHandler(requestStream, context, continuation);
                LogInformation(context.Method, requestString, response);
            }
            catch (Exception e)
            {
                return LogExceptionAndReturnResponse(context.Method, requestString, response, e);
            }
            return response;
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                //因此地方不能正確的讀到Request.Body或Response.Body
                //且也不能去讀requestStream & responseStream，會造成主邏輯會讀到空，stream類型的log可能要改成IActionFilter
                await base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
            }
            catch (Exception e)
            {
                LogExceptionAndReturnResponse(context.Method, requestStream.ToString(), responseStream.ToString(), e);
            }
        }


        private void LogInformation<TRequest, TResponse>(string method, TRequest request, TResponse response)
        {
            responseTime = DateTime.UtcNow;
            _logger.LogInformation(JsonConvert.SerializeObject(
                    new
                    {
                        method,
                        request,
                        requestTime,
                        response,
                        responseTime,
                        excuteTime = GetExcuteSecond()
                    }
                ));
        }
        private TResponse LogExceptionAndReturnResponse<TRequest, TResponse>(string method, TRequest request, TResponse response, Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Exception from interceptor");
            Console.WriteLine(e.ToString());

            var instance = Activator.CreateInstance(typeof(TResponse));
            if (instance is BaseResponse)
            {
                instance.GetType().GetProperty(nameof(BaseResponse.Success)).SetValue(instance, false);
                instance.GetType().GetProperty(nameof(BaseResponse.Code)).SetValue(instance, "99999");
                instance.GetType().GetProperty(nameof(BaseResponse.Message)).SetValue(instance, e.Message);
            }

            responseTime = DateTime.UtcNow;
            _logger.LogError(JsonConvert.SerializeObject(
                   new
                   {
                       method,
                       request,
                       requestTime,
                       response = instance,
                       responseTime,
                       excuteTime = GetExcuteSecond(),
                       exception = e.ToString()
                   }
               ));

            return (TResponse)instance;
        }

        private double GetExcuteSecond()
                => Math.Round((responseTime - requestTime).TotalMilliseconds) / 1000;


    }
}