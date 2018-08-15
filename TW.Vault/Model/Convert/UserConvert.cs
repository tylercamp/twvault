﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Model.Convert
{
    public static class UserConvert
    {
        public static JSON.User ModelToJson(Scaffold.User user, String playerName = null, String tribeName = null)
        {
            var result = new JSON.User();
            result.PlayerId = user.PlayerId;
            result.Priveleges = user.PermissionsLevel;
            result.PlayerName = playerName;
            result.TribeName = tribeName;
            result.Key = user.AuthToken.ToString();

            return result;
        }
    }
}
