using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbqtExtensions.StreamerBot
{
    public interface IStreamerBotEventHandler
    {
        void Execute(string obj);
    }
}
