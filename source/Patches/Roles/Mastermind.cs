using System.Collections.Generic;

namespace TownOfUs.Roles
{
    public class Mastermind : Role
    {
        public List<byte> Reported = new List<byte>();

        public Mastermind(PlayerControl player) : base(player)
        {
            Name = "Mastermind";
            ImpostorText = () => "Know the roles of bodies you report";
            TaskText = () => "Know the roles of bodies you report";
            Color = Palette.ImpostorRed;
            RoleType = RoleEnum.Mastermind;
            Faction = Faction.Impostors;
        }
    }
}