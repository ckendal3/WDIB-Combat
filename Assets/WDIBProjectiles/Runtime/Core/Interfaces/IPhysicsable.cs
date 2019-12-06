using Unity.Mathematics;

namespace WDIB.Interfaces
{
    public interface IPhysicsObject
    {
        /// <summary>
        /// Apply a constant force to the object
        /// </summary>
        /// <param name="force"> How much force to apply </param>
        void AddForce(float force);

        /// <summary>
        /// Apply an impulse force such as an explosion
        /// </summary>
        /// <param name="force"> how much force to apply</param>
        /// <param name="position"> the origin point of the impulse </param>
        void AddImpulseForce(float force, float3 origin);

        /// <summary>
        /// Freeze an object so it is not interacted with by normal physics 
        /// </summary>
        /// <param name="value"> How long the object should be frozen. </param>
        void FreezeForDuration(float value);
    }
}