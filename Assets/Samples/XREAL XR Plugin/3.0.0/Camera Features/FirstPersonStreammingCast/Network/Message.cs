using System;

namespace Unity.XR.XREAL.Samples.NetWork
{
    /// <summary> Net msg type. </summary>
    public enum MessageType
    {
        /// <summary> Empty type. </summary>
        None = 0,

        /// <summary> Connect server. </summary>
        Connected = 1,
        /// <summary> Disconnect from server. </summary>
        Disconnect = 2,

        /// <summary> Heart beat. </summary>
        HeartBeat = 3,
        /// <summary> Enter room. </summary>
        EnterRoom = 4,
        /// <summary> Enter room. </summary>
        ExitRoom = 5,

        /// <summary> An enum constant representing the update camera Parameter option. </summary>
        UpdateCameraParam = 6,

        /// <summary> Used to synchronization message with the server. </summary>
        MessageSynchronization = 7,
    }

    /// <summary> (Serializable) an enter room data. </summary>
    [Serializable]
    public class EnterRoomData
    {
        /// <summary> Enter room result. </summary>
        public bool result;
    }

    /// <summary> (Serializable) an exit room data. </summary>
    [Serializable]
    public class ExitRoomData
    {
        /// <summary> Exit room result. </summary>
        public bool Suc;
    }

    /// <summary> (Serializable) a camera parameter. </summary>
    [Serializable]
    public class CameraParam
    {
        /// <summary> Camera fov. </summary>
        public Fov4f fov;
    }


    /// <summary> (Serializable) a fov 4f. </summary>
    [Serializable]
    public class Fov4f
    {
        /// <summary> The left. </summary>
        public double left;
        /// <summary> The right. </summary>
        public double right;
        /// <summary> The top. </summary>
        public double top;
        /// <summary> The bottom. </summary>
        public double bottom;

        /// <summary> Default constructor. </summary>
        public Fov4f() { }

        /// <summary> Constructor. </summary>
        /// <param name="l"> A double to process.</param>
        /// <param name="r"> A double to process.</param>
        /// <param name="t"> A double to process.</param>
        /// <param name="b"> A double to process.</param>
        public Fov4f(double l, double r, double t, double b)
        {
            this.left = l;
            this.right = r;
            this.top = t;
            this.bottom = b;
        }

        /// <summary> Convert this object into a string representation. </summary>
        /// <returns> A string that represents this object. </returns>
        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", left, right, top, bottom);
        }
    }
}
