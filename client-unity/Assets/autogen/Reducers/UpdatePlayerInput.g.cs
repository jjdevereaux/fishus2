// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

#nullable enable

using System;
using SpacetimeDB.ClientApi;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.Types
{
    public sealed partial class RemoteReducers : RemoteBase
    {
        public delegate void UpdatePlayerInputHandler(ReducerEventContext ctx, DbVector3 direction);
        public event UpdatePlayerInputHandler? OnUpdatePlayerInput;

        public void UpdatePlayerInput(DbVector3 direction)
        {
            conn.InternalCallReducer(new Reducer.UpdatePlayerInput(direction), this.SetCallReducerFlags.UpdatePlayerInputFlags);
        }

        public bool InvokeUpdatePlayerInput(ReducerEventContext ctx, Reducer.UpdatePlayerInput args)
        {
            if (OnUpdatePlayerInput == null) return false;
            OnUpdatePlayerInput(
                ctx,
                args.Direction
            );
            return true;
        }
    }

    public abstract partial class Reducer
    {
        [SpacetimeDB.Type]
        [DataContract]
        public sealed partial class UpdatePlayerInput : Reducer, IReducerArgs
        {
            [DataMember(Name = "direction")]
            public DbVector3 Direction;

            public UpdatePlayerInput(DbVector3 Direction)
            {
                this.Direction = Direction;
            }

            public UpdatePlayerInput()
            {
                this.Direction = new();
            }

            string IReducerArgs.ReducerName => "update_player_input";
        }
    }

    public sealed partial class SetReducerFlags
    {
        internal CallReducerFlags UpdatePlayerInputFlags;
        public void UpdatePlayerInput(CallReducerFlags flags) => UpdatePlayerInputFlags = flags;
    }
}
