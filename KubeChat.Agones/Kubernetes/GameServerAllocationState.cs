using System;
using System.Linq;

namespace KubeChat.Agones.Kubernetes
{
    public class GameServerAllocationState : IEquatable<GameServerAllocationState>
    {
        public const string Allocated = "Allocated";
        public const string UnAllocated = "UnAllocated";
        public const string Contention = "Contention";

        private static readonly string[] States = { Allocated, UnAllocated, Contention };

        private readonly string _state;

        public GameServerAllocationState(string state)
        {
            if (string.IsNullOrEmpty(state))
            {
                throw new ArgumentException($"State is null or empty", nameof(state));
            }
            else if (States.Contains(state))
            {
                _state = state;
            }
            else
            {
                throw new ArgumentException($"State is not {string.Join(", ", States)}", nameof(state));
            }
        }

        public override string ToString()
        {
            return _state;
        }

        public static implicit operator GameServerAllocationState(string value)
        {
            return new GameServerAllocationState(value);
        }

        public static implicit operator string(GameServerAllocationState value)
        {
            return value._state;
        }

        public bool Equals(GameServerAllocationState other)
        {
            return _state.Equals(other._state);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GameServerAllocationState);
        }

        public override int GetHashCode()
        {
            return _state.GetHashCode();
        }

        public static bool operator ==(GameServerAllocationState lhs, GameServerAllocationState rhs)
        {
            return lhs == rhs;
        }

        public static bool operator !=(GameServerAllocationState lhs, GameServerAllocationState rhs)
        {
            return lhs != rhs;
        }
    }
}