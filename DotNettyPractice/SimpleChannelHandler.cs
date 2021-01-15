﻿namespace DotNettyPractice
{
    using System;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    public class SimpleChannelHandler : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;
            if (buffer != null)
            {
                Console.WriteLine($"Received from {context.Channel.RemoteAddress}: {buffer.ToString(Encoding.UTF8)}");
            }
            context.WriteAsync(message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }
    }
}