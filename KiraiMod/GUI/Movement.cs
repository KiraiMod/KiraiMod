using UnityEngine;
using UnityEngine.UI;
using static KiraiMod.Managers.GUIManager;

namespace KiraiMod.GUI
{
    public static class Movement
    {
        public static Toggle flight;
        public static Toggle directional;
        public static Toggle noclip;
        public static KiraiSlider flightSpeed;

        public static void Setup(Transform self)
        {
            Transform Body = self.Find("Body");

            Common.Window.Create(self, self.Find("Title"), Body)
                .Dragable()
                .Closable();

            (flight = Body.Find("Flight").GetComponent<Toggle>()).On(state => Modules.Movement.State = state);
            (directional = Body.Find("Directional").GetComponent<Toggle>()).On(state => Modules.Movement.directional.Value = state);
            (noclip = Body.Find("NoClip").GetComponent<Toggle>()).On(state => Modules.Movement.noclip.Value = state);
            (flightSpeed = Body.Find("FlightSpeed").GetKiraiSlider()).slider.On(value => Modules.Movement.speed.Value = value);
            Body.Find("Locomotion").GetComponent<Button>().On(Modules.Movement.UseLegacyLocomotion);
            Body.Find("Position/Save").GetComponent<Button>().On(Modules.Movement.SavePosition);
            Body.Find("Position/Load").GetComponent<Button>().On(Modules.Movement.LoadPosition);

            Modules.Movement.directional.GUIBind(directional);
            Modules.Movement.noclip.GUIBind(noclip);
            Modules.Movement.speed.GUIBind(flightSpeed);
        }
    }
}
