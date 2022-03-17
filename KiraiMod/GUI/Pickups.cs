using KiraiMod.Managers;
using UnityEngine;
using UnityEngine.UI;
using static KiraiMod.Managers.GUIManager;

namespace KiraiMod.GUI
{
    public static class Pickups
    {
        public static void Setup(Transform Self)
        {
            Transform Body = Self.Find("Body");

            Common.Window.Create(Self)
                .Dragable()
                .Closable(Body.gameObject)
                .Pinnable();

            Modules.Pickups.Modifiers.unlock.GUIBind(Body.Find("Unlock").GetComponent<Toggle>());
            Modules.Pickups.Modifiers.theft.GUIBind(Body.Find("Theft").GetComponent<Toggle>());
            Modules.Pickups.Modifiers.rotate.GUIBind(Body.Find("Rotate").GetComponent<Toggle>());
            Modules.Pickups.Modifiers.reach.GUIBind(Body.Find("Reach").GetComponent<Toggle>());
            Modules.Pickups.Modifiers.boost.GUIBind(Body.Find("Boost").GetComponent<Toggle>());

            Body.Find("Freeze");
            Body.Find("Fetch");
            Body.Find("Drop");

            Modules.Pickups.Orbit.State.Bind(Body.Find("Orbit").GetComponent<Toggle>());
            Body.Find("Head");
            Modules.Pickups.Orbit.Speed.GUIBind(BoundSlider.Create(Body.Find("Speed").GetComponent<Slider>(), Body.Find("SpeedText").GetComponent<Text>()));
            Modules.Pickups.Orbit.Distance.GUIBind(BoundSlider.Create(Body.Find("Distance").GetComponent<Slider>(), Body.Find("DistanceText").GetComponent<Text>()));
            Modules.Pickups.Orbit.Offset.GUIBind(BoundSlider.Create(Body.Find("Offset").GetComponent<Slider>(), Body.Find("OffsetText").GetComponent<Text>()));
        }
    }
}
