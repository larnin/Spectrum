using System;
using Spectrum.API.Game.EventArgs.Vehicle;
using Spectrum.API.Helpers;
using Spectrum.API.TypeWrappers;
using UnityEngine;

namespace Spectrum.API.Game.Vehicle
{
    public class LocalVehicle
    {
        private static CarLogic VehicleLogic { get; set; }
        private static bool CanOperateOnVehicle => VehicleLogic != null;

        public static Screen Screen { get; private set; }
        public static HUD HUD { get; private set; }

        public static float HeatLevel
        {
            get
            {
                UpdateObjectReferences();
                if (CanOperateOnVehicle)
                    return VehicleLogic.Heat_;

                return 0f;
            }
        }

        public static float VelocityKPH
        {
            get
            {
                UpdateObjectReferences();
                if (CanOperateOnVehicle)
                    return VehicleLogic.CarStats_.GetKilometersPerHour();

                return 0f;
            }
        }

        public static float VelocityMPH
        {
            get
            {
                UpdateObjectReferences();
                if (CanOperateOnVehicle)
                    return VehicleLogic.CarStats_.GetMilesPerHour();

                return 0f;
            }
        }

        public static event EventHandler BeforeExploded;
        public static event EventHandler BeforeSplit;
        public static event EventHandler CheckpointPassed;
        public static event EventHandler<ImpactEventArgs> Collided;
        public static event EventHandler<DestroyedEventArgs> Destroyed;
        public static event EventHandler<DestroyedEventArgs> Exploded;
        public static event EventHandler FinishHit;
        public static event EventHandler<HonkEventArgs> Honked;
        public static event EventHandler Jumped;
        public static event EventHandler SpecialModeEvent;
        public static event EventHandler<SplitEventArgs> Split;
        public static event EventHandler<TrickCompleteEventArgs> TrickCompleted;
        public static event EventHandler WingsOpened;
        public static event EventHandler WingsClosed;
        public static event EventHandler WingsEnabled;
        public static event EventHandler WingsDisabled;

        static LocalVehicle()
        {
            Screen = new Screen();
            HUD = new HUD();

            Events.Car.PreExplode.SubscribeAll((sender, data) =>
            {
                BeforeExploded?.Invoke(null, System.EventArgs.Empty);
            });

            Events.Car.PreSplit.SubscribeAll((sender, data) =>
            {
                BeforeSplit?.Invoke(null, System.EventArgs.Empty);
            });

            Events.Car.CheckpointHit.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    CheckpointPassed?.Invoke(null, System.EventArgs.Empty);
                }
            });

            Events.Car.Death.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    var eventArgs = new DestroyedEventArgs((DestructionCause)data.causeOfDeath);
                    Destroyed?.Invoke(null, eventArgs);
                }
            });

            Events.Car.Explode.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    var eventArgs = new DestroyedEventArgs((DestructionCause)data.causeOfDeath);
                    Exploded?.Invoke(null, eventArgs);
                }
            });

            Events.RaceEnd.LocalCarHitFinish.Subscribe(data =>
            {
                FinishHit?.Invoke(null, System.EventArgs.Empty);
            });

            Events.Car.Horn.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    var eventArgs = new HonkEventArgs(data.hornPercent_, new Position(data.position_.x, data.position_.y, data.position_.z));
                    Honked?.Invoke(null, eventArgs);
                }
            });

            Events.Car.Impact.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    var eventArgs = new ImpactEventArgs(data.speed_, new Position(data.pos_.x, data.pos_.y, data.pos_.z), data.impactedCollider_.name);
                    Collided?.Invoke(null, eventArgs);
                }
            });

            Events.Car.Jump.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    Jumped?.Invoke(null, System.EventArgs.Empty);
                }
            });

            Events.Car.ModeSpecial.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    SpecialModeEvent?.Invoke(null, System.EventArgs.Empty);
                }  
            });

            Events.Car.Split.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    var eventArgs = new SplitEventArgs(data.penetration, data.separationSpeed);
                    Split?.Invoke(null, eventArgs);
                }
            });

            Events.Car.TrickComplete.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    var eventArgs = new TrickCompleteEventArgs(data.boost_, data.boostPercent_, data.boostTime_, data.cooldownPercent_, data.points_, data.rechargeAmount_);
                    TrickCompleted?.Invoke(null, eventArgs);
                }
            });

            Events.Car.WingsStateChange.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    if (data.open_)
                    {
                        WingsOpened?.Invoke(null, System.EventArgs.Empty);
                    }
                    else
                    {
                        WingsClosed?.Invoke(null, System.EventArgs.Empty);
                    }
                }
            });

            Events.Car.WingsAbilityStateChanged.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    if (data.enabled_)
                    {
                        WingsEnabled?.Invoke(null, System.EventArgs.Empty);
                    }
                    else
                    {
                        WingsDisabled?.Invoke(null, System.EventArgs.Empty);
                    }
                }  
            });
        }

        public static void SetJetFlamesColor(string hexColor)
        {
            UpdateObjectReferences();
            if (CanOperateOnVehicle && VehicleLogic.CarLogicLocal_ != null)
            {
                var jets = Utilities.GetPrivate<JetsGadget>(VehicleLogic.CarLogicLocal_, "jetsGadget_");
                if (jets != null)
                {
                    foreach (var flame in jets.flames_)
                    {
                        flame.flame_.SetCustomColor(hexColor.ToColor());
                    }
                }
            }
        }

        public static void SetBoostFlameColor(string hexColor)
        {
            UpdateObjectReferences();
            if (CanOperateOnVehicle && VehicleLogic.CarLogicLocal_ != null)
            {
                var booster = Utilities.GetPrivate<BoostGadget>(VehicleLogic.CarLogicLocal_, "boostGadget_");
                if (booster != null)
                {
                    foreach (var flame in booster.flames_)
                    {
                        flame.SetCustomColor(hexColor.ToColor());
                    }
                }
            }
        }

        public static void SetWingTrailsColor(string hexColor)
        {
            UpdateObjectReferences();
            if (CanOperateOnVehicle && VehicleLogic.CarLogicLocal_ != null)
            {
                var wingsGadget = Utilities.GetPrivate<WingsGadget>(VehicleLogic.CarLogicLocal_, "wingsGadget_");
                if (wingsGadget != null)
                {
                    var wingTrailHelpers = Utilities.GetPrivate<WingTrailHelper[]>(wingsGadget, "wingTrails_");
                    if (wingTrailHelpers != null)
                    {
                        foreach (var wingTrailHelper in wingTrailHelpers)
                        {
                            var wingTrail = Utilities.GetPrivate<WingTrail>(wingTrailHelper, "wingTrail_");
                            if (wingTrail != null)
                            {
                                wingTrail.GetComponent<Renderer>().material.color = hexColor.ToColor();
                            }
                        }
                    }
                }
            }
        }

        private static void UpdateObjectReferences()
        {
            VehicleLogic = Utilities.FindLocalCar().GetComponent<CarLogic>();
        }
    }
}
