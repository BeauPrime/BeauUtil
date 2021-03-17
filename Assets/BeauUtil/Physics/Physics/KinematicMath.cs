/*
 * Copyright (C) 2017-2021. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 March 2021
 * 
 * File:    KinematicMath.cs
 * Purpose: 3d Kinematic math functions.
 */

#if CSHARP_7_3_OR_NEWER
#define EXPANDED_REFS
#endif // CSHARP_7_3_OR_NEWER

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BeauUtil
{
    static public class KinematicMath
    {
        /// <summary>
        /// Minimum speed at which velocity is snapped to 0.
        /// </summary>
        static public float MinSpeed = 1f / 64;

        /// <summary>
        /// Ticks the kinematic property block forward by a certain delta time.
        /// This will integrate all kinematic properties.
        /// </summary>
        static public Vector3 Integrate(ref KinematicState ioProperties, ref KinematicConfig inConfig, float inDeltaTime)
        {
            ApplyLimits(ref ioProperties, ref inConfig);
            Vector3 offset = IntegratePosition(ref ioProperties, ref inConfig, inDeltaTime);
            IntegrateVelocity(ref ioProperties, ref inConfig, inDeltaTime);
            ApplyLimits(ref ioProperties, ref inConfig);
            return offset;
        }

        /// <summary>
        /// Integrates position with velocity, acceleration, and gravity.
        /// </summary>
        static public Vector3 IntegratePosition(ref KinematicState ioProperties, ref KinematicConfig inConfig, float inDeltaTime)
        {
            float t2 = inDeltaTime * inDeltaTime;
            float dx = (ioProperties.Velocity.x * inDeltaTime)
                + (((inConfig.Acceleration.x + inConfig.Gravity.x * ioProperties.GravityMultiplier) * t2 * 0.5f));
            float dy = (ioProperties.Velocity.y * inDeltaTime)
                + (((inConfig.Acceleration.y + inConfig.Gravity.y * ioProperties.GravityMultiplier) * t2 * 0.5f));
            float dz = (ioProperties.Velocity.z * inDeltaTime)
                + (((inConfig.Acceleration.z + inConfig.Gravity.z * ioProperties.GravityMultiplier) * t2 * 0.5f));
            return new Vector3(dx, dy, dz);
        }

        /// <summary>
        /// Integrates velocity, accounting for acceleration, gravity, friction, and drag.
        /// </summary>
        static public void IntegrateVelocity(ref KinematicState ioProperties, ref KinematicConfig inConfig, float inDeltaTime)
        {
            // acceleration
            float x = ioProperties.Velocity.x + (inConfig.Acceleration.x + inConfig.Gravity.x * ioProperties.GravityMultiplier) * inDeltaTime;
            float y = ioProperties.Velocity.y + (inConfig.Acceleration.y + inConfig.Gravity.y * ioProperties.GravityMultiplier) * inDeltaTime;
            float z = ioProperties.Velocity.z + (inConfig.Acceleration.z + inConfig.Gravity.z * ioProperties.GravityMultiplier) * inDeltaTime;
            
            // friction
            x = x < 0 ? Math.Min(x + inConfig.Friction.x * inDeltaTime, 0) : Math.Max(x - inConfig.Friction.x * inDeltaTime, 0);
            y = y < 0 ? Math.Min(y + inConfig.Friction.y * inDeltaTime, 0) : Math.Max(y - inConfig.Friction.y * inDeltaTime, 0);
            z = z < 0 ? Math.Min(z + inConfig.Friction.z * inDeltaTime, 0) : Math.Max(z - inConfig.Friction.z * inDeltaTime, 0);
            
            // drag
            float drag = inConfig.Drag > 0 ? (float) Math.Exp(-inConfig.Drag * inDeltaTime) : 1;
            x *= drag;
            y *= drag;
            z *= drag;

            // copy
            ioProperties.Velocity.x = x;
            ioProperties.Velocity.y = y;
            ioProperties.Velocity.z = z;
        }

        /// <summary>
        /// Applies speed limits to the given property block.
        /// </summary>
        static public void ApplyLimits(ref KinematicState ioProperties, ref KinematicConfig inConfig)
        {
            float speed2 = ioProperties.Velocity.sqrMagnitude;
            float high = inConfig.MaxSpeed;
            float high2 = high * high;
            float low2 = MinSpeed * MinSpeed;

            if (speed2 < low2)
            {
                ioProperties.Velocity.x = ioProperties.Velocity.y = 0;
            }
            else if (high2 > 0 && speed2 > high2)
            {
                ioProperties.Velocity.Normalize();
                ioProperties.Velocity.x *= high;
                ioProperties.Velocity.y *= high;
                ioProperties.Velocity.z *= high;
            }
        }
    }

    /// <summary>
    /// Property block containing properties for 3d kinematics.
    /// This is likely to change every frame.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct KinematicState
    {
        public Vector3 Velocity;
        public float GravityMultiplier;
    }

    /// <summary>
    /// Constraints for 3d kinematic objects.
    /// These are unlikely to change frame-by-frame.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct KinematicConfig
    {
        [Header("Forces")]
        public Vector3 Acceleration;
        public Vector3 Gravity;

        [Header("Friction")]
        public float Drag;
        public Vector3 Friction;

        [Header("Limits")]
        public float MaxSpeed;
    }
}