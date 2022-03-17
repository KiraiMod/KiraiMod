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
        public static BoundSlider flightSpeed;

        public static void Setup(Transform self)
        {
            Transform Body = self.Find("Body");

            Common.Window.Create(self)
                .Dragable()
                .Closable(Body.gameObject)
                .Pinnable();

            Modules.Movement.state.Bind(flight = Body.Find("Flight").GetComponent<Toggle>());
            Modules.Movement.directional.GUIBind(directional = Body.Find("Directional").GetComponent<Toggle>());
            Modules.Movement.noclip.GUIBind(noclip = Body.Find("NoClip").GetComponent<Toggle>());
            Modules.Movement.speed.GUIBind(flightSpeed = BoundSlider.Create(Body.Find("FlightSpeed")));
            Body.Find("Locomotion").GetComponent<Button>().On(Modules.Movement.UseLegacyLocomotion);
            Body.Find("Position/Save").GetComponent<Button>().On(Modules.Movement.SavePosition);
            Body.Find("Position/Load").GetComponent<Button>().On(Modules.Movement.LoadPosition);

            Modules.Movement.directional.GUIBind(directional);
            Modules.Movement.noclip.GUIBind(noclip);
            Modules.Movement.speed.GUIBind(flightSpeed);
        }
    }
}
