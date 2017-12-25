using MiNET.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerSave
{
    public class PlayerSaveMain : Plugin
    {
        protected override void OnEnable()
        {
            Context.Server.PlayerFactory.PlayerCreated += PlayerFactory_PlayerCreated;
        }

        private void PlayerFactory_PlayerCreated(object sender, MiNET.PlayerEventArgs e)
        {
            e.Player.PlayerJoin += Player_PlayerJoin;
            e.Player.PlayerLeave += Player_PlayerLeave;
        }

        private void Player_PlayerLeave(object sender, MiNET.PlayerEventArgs e)
        {
            e.Player.Save(true);
        }

        private void Player_PlayerJoin(object sender, MiNET.PlayerEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
