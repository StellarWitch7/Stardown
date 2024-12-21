using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stardown.Core.Services
{
    internal class CommunicationService
    {
        public static CommunicationService Instance = new CommunicationService();
        private object _syncLock = new object();
        private int _lastIdReceived = 0;
        private Uri _baseAddress = new Uri("http://1.1.1.4:60080/api/chat/", UriKind.Absolute);

        public CommunicationService()
        {
            Task.Run(() => RunThreadAsync());
        }

        private async Task RunThreadAsync()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        using (var client = new HttpClient { BaseAddress = _baseAddress })
                        {
                            int latestId = await client.GetFromJsonAsync<int>("i");

                            if (latestId > _lastIdReceived)
                            {
                                var getId = latestId++;
                                var message = await client.GetFromJsonAsync<Message>($"m/{getId}");
                                _lastIdReceived = message.Id;
                                OnMessageReceived(message);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                    }

                    Thread.Sleep(1000);
                }
            }
            catch(Exception ex)
            {

            }
        }

        public void SendMessage(Message message)
        {
            Task.Run(() => SendMessageAsync(message));
        }

        public async Task SendMessageAsync(Message message)
        {
            using (var client = new HttpClient { BaseAddress = _baseAddress })
            {
                HttpResponseMessage response = await client.PostAsJsonAsync<Message>("m", message);
            }
        }

        protected virtual void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }

        public event MessageReceivedEventHandler? MessageReceived;
    }

    internal delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs args);

    internal class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(Message message)
        {
            this.Message = message;
        }

        public Message Message { get; set; }
    }
}
