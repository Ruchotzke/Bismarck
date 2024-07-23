using System;
using UnityEngine;

namespace bismarck.meshing
{
    /// <summary>
    /// A meshing vertex.
    /// </summary>
    public struct Vertex
    {
        /// <summary>
        /// The position of this vertex.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The normal for this vertex.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The texture coordinate of this vertex.
        /// </summary>
        public Vector2 Texture;

        /// <summary>
        /// Generate a new vertex.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <param name="texture"></param>
        public Vertex(Vector3 position, Vector3 normal = default, Vector2 texture = default)
        {
            Position = position;
            Normal = normal;
            Texture = texture;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex other)
            {
                return other.Position == this.Position && other.Normal == this.Normal && other.Texture == this.Texture;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Normal, Texture);
        }
    }
}