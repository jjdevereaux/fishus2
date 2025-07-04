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
        public delegate void MoveAllPlayersHandler(ReducerEventContext ctx, MoveAllPlayersTimer timer);
        public event MoveAllPlayersHandler? OnMoveAllPlayers;

        public void MoveAllPlayers(MoveAllPlayersTimer timer)
        {
            conn.InternalCallReducer(new Reducer.MoveAllPlayers(timer), this.SetCallReducerFlags.MoveAllPlayersFlags);
        }

        public bool InvokeMoveAllPlayers(ReducerEventContext ctx, Reducer.MoveAllPlayers args)
        {
            if (OnMoveAllPlayers == null) return false;
            OnMoveAllPlayers(
                ctx,
                args.Timer
            );
            return true;
        }
    }

    public abstract partial class Reducer
    {
        [SpacetimeDB.Type]
        [DataContract]
        public sealed partial class MoveAllPlayers : Reducer, IReducerArgs
        {
            [DataMember(Name = "timer")]
            public MoveAllPlayersTimer Timer;

            public MoveAllPlayers(MoveAllPlayersTimer Timer)
            {
                this.Timer = Timer;
            }

            public MoveAllPlayers()
            {
                this.Timer = new();
            }

            string IReducerArgs.ReducerName => "move_all_players";
        }
    }

    public sealed partial class SetReducerFlags
    {
        internal CallReducerFlags MoveAllPlayersFlags;
        public void MoveAllPlayers(CallReducerFlags flags) => MoveAllPlayersFlags = flags;
    }
}
