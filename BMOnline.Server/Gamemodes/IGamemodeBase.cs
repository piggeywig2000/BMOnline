using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOnline.Common.Gamemodes;

namespace BMOnline.Server.Gamemodes
{
    internal interface IGamemodeBase
    {
        OnlineGamemode GamemodeType { get; }
        void AddUser(User user);
        void RemoveUser(ushort user);
        bool UserInGamemode(ushort user);
    }
}
