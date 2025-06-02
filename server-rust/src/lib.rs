pub mod math;

use math::{DbVector2, DbVector3};
use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};

// INCLUDES ABOVE \\
// TABLES BELOW   \\

// CONFIGURATION TABLE
#[spacetimedb::table(name = config, public)]
pub struct Config {
    #[primary_key]
    pub id: u32,
    pub world_size: u64,
    pub tile_size: u64,
}

// ENTITY TABLE
#[spacetimedb::table(name = entity, public)]
#[derive(Debug, Clone)]
pub struct Entity {
    #[auto_inc]
    #[primary_key]
    pub entity_id: u32,
    pub position: DbVector3,
}

// ITEM TABLE
#[spacetimedb::table(name = items, public)]
#[derive(Debug, Clone)]
pub struct Item {
    #[auto_inc]
    #[primary_key]
    pub item_id: u32,
    pub owner_id: u32, // entity id reference of owner
    pub item_type: u32,
    pub item_position: u32,
    // add modifiers, quantifiers as fields later if necessary
}

// ITEM TYPE TABLE
#[spacetimedb::table(name = itemtype, public)]
#[derive(Debug, Clone)]
pub struct ItemType {
    #[auto_inc]
    #[primary_key]
    pub item_type_id: u32,
}


// FISHERMAN TABLE
#[spacetimedb::table(name = fisherman, public)]
pub struct Fisherman {
    #[primary_key]
    pub entity_id: u32,
    #[index(btree)]
    pub player_id: u32,
    pub direction: DbVector3,
    pub speed: f32,
}

// PLAYER TABLE
#[spacetimedb::table(name = player, public)]
#[spacetimedb::table(name = logged_out_player)]
#[derive(Debug, Clone)]
pub struct Player {
    #[primary_key]
    identity: Identity,
    #[unique]
    #[auto_inc]
    player_id: u32,
    name: String,
}

// AREA TILE TABLE
#[spacetimedb::table(name = tile, public)]
#[derive (Debug, Clone)]
pub struct Tile {
    #[primary_key]
    pub entity_id: u32,
//    pub tile_type: u32,
}

// PLAYER MOVEMENT SCHEDULE
#[spacetimedb::table(name = move_all_players_timer, scheduled(move_all_players))]
pub struct MoveAllPlayersTimer {
    #[primary_key]
    #[auto_inc]
    scheduled_id: u64,
    scheduled_at: spacetimedb::ScheduleAt,
}

// TABLES ABOVE   \\
// REDUCERS BELOW \\

// DEBUG REDUCER
#[spacetimedb::reducer]
pub fn debug(ctx: &ReducerContext) -> Result<(), String> {
    log::debug!("This reducer was called by {}.", ctx.sender);
    Ok(())
}

// CLIENT CONNECT REDUCER
#[spacetimedb::reducer(client_connected)]
pub fn connect(ctx: &ReducerContext) -> Result<(), String> {
    if let Some(player) = ctx.db.logged_out_player().identity().find(&ctx.sender) {
        ctx.db.player().insert(player.clone());
        ctx.db
            .logged_out_player()
            .identity()
            .delete(&player.identity);
    } else {
        ctx.db.player().try_insert(Player {
            identity: ctx.sender,
            player_id: 0,
            name: String::new(),
        })?;
    }
    Ok(())
}

// CLIENT DISCONNECT REDUCER
#[spacetimedb::reducer(client_disconnected)]
pub fn disconnect(ctx: &ReducerContext) -> Result<(), String> {
    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found")?;
    let player_id = player.player_id;
    ctx.db.logged_out_player().insert(player);
    ctx.db.player().identity().delete(&ctx.sender);
    for fisherman in ctx.db.fisherman().player_id().filter(&player_id) {
        ctx.db.entity().entity_id().delete(&fisherman.entity_id);
        ctx.db.fisherman().entity_id().delete(&fisherman.entity_id);
    }
    Ok(())
}

// INITIALIZATION REDUCER (Runs once on DB creation)
#[spacetimedb::reducer(init)]
pub fn init(ctx: &ReducerContext) -> Result<(), String> {
    log::info!("Initializing...");
    ctx.db.config().try_insert(Config {
        id: 0,
        world_size: 100,
        tile_size: 10,
    })?;
    ctx.db
    .move_all_players_timer()
    .try_insert(MoveAllPlayersTimer {
        scheduled_id: 0,
        scheduled_at: ScheduleAt::Interval(Duration::from_millis(50).into()),
    })?;
    
    // POPULATE TILES
    let world_size = ctx
    .db
    .config()
    .id()
    .find(0)
    .ok_or("Config not found")?
    .world_size;
    let tile_size = ctx
    .db
    .config()
    .id()
    .find(0)
    .ok_or("Config not found")?
    .tile_size;

    let mut tile_count = ctx.db.tile().count();
    let mut i = 0;
    let mut j = 0;
    while tile_count < world_size {
        let x = i as f32 * tile_size as f32;
        let y = 1.0;
        let z = j as f32 * tile_size as f32;
        let entity = ctx.db.entity().try_insert(Entity {
        entity_id: 0,
        position: DbVector3 { x , y , z},
    })?;
        ctx.db.tile().try_insert(Tile {
        entity_id: entity.entity_id,
    })?;
        tile_count += 1;
        if i == world_size / tile_size {
        j += 1;
        i = 0;
        } else {i += 1;}
        log::info!("Spawned tile! {}", entity.entity_id);
    }
    Ok(())
}

// PLAYER ENTER GAME REDUCER
#[spacetimedb::reducer]
pub fn enter_game(ctx: &ReducerContext, name: String) -> Result<(), String> {
    log::info!("Creating player with name {}", name);
    let mut player: Player = ctx.db.player().identity().find(ctx.sender).ok_or("")?;
    let player_id = player.player_id;
    player.name = name;
    ctx.db.player().identity().update(player);
    spawn_player_fisherman_initial(ctx, player_id)?;

    Ok(())
}

// PLAYER MOVEMENT UPDATE REDUCER
#[spacetimedb::reducer]
pub fn update_player_input(ctx: &ReducerContext, direction: DbVector3) -> Result<(), String> {
    let player = ctx
        .db
        .player()
        .identity()
        .find(&ctx.sender)
        .ok_or("Player not found")?;
    for mut fisherman in ctx.db.fisherman().player_id().filter(&player.player_id) {
        fisherman.direction = direction.normalized();
        fisherman.speed = direction.magnitude().clamp(0.0, 1.0) * START_PLAYER_SPEED as f32;
        ctx.db.fisherman().entity_id().update(fisherman);
    }
    Ok(())
}

// PLAYER POSITION UPDATE REDUCER
#[spacetimedb::reducer]
pub fn move_all_players(ctx: &ReducerContext, timer: MoveAllPlayersTimer) -> Result<(), String> {
    let world_size = ctx
        .db
        .config()
        .id()
        .find(0)
        .ok_or("Config not found")?
        .world_size;
    let tile_size = ctx
        .db
        .config()
        .id()
        .find(0)
        .ok_or("Config not found")?
        .tile_size;
    let spacesize = world_size * tile_size;
    // Handle player input
    for fisherman in ctx.db.fisherman().iter() {
        let fisherman_entity = ctx.db.entity().entity_id().find(&fisherman.entity_id);
        if !fisherman_entity.is_some() {
            continue;
        }
        let mut fisherman_entity = fisherman_entity.unwrap();
        let direction = fisherman.direction * fisherman.speed;
        let new_pos =
            fisherman_entity.position + direction;
        let min = spacesize as f32 * -1.0;
        let max = spacesize as f32;
        fisherman_entity.position.x = new_pos.x.clamp(min, max);
        fisherman_entity.position.y = new_pos.y.clamp(min, max);
        fisherman_entity.position.z = new_pos.z.clamp(min, max);
        ctx.db.entity().entity_id().update(fisherman_entity);
    }

    Ok(())
}

// ADD ITEM REDUCER
#[spacetimedb::reducer]
pub fn add_item(ctx: &ReducerContext, itemtype: u32, itempos: u32) -> Result<(), String> {
    let player = ctx
    .db
    .player()
    .identity()
    .find(&ctx.sender)
    .ok_or("Player not found")?;

    ctx.db.items().try_insert(Item {
        item_id: 0,
        owner_id: player.entity_id,
        item_type: itemtype,
        item_position: itempos,
    })?
    Ok(())
}

// REMOVE ITEM REDUCER
#[spacetimedb::reducer]
pub fn remove_item(ctx: &ReducerContext, itemid: u32) -> Result<(), String>{
ctx.db.items().item_id().delete(&itemid);
Ok(())
}




// REDUCERS ABOVE \\
// CONSTANTS BELOW \\

const START_PLAYER_SPEED: u32 = 2;

// CONSTANTS ABOVE \\
// DERIVES BELOW  \\

///////////////////////////////////
///////////////////////////////////

// DERIVES ABOVE   \\
// FUNCTIONS BELOW \\

// INITIAL FISHERMAN SPAWN FUNCTION
fn spawn_player_fisherman_initial(ctx: &ReducerContext, player_id: u32) -> Result<Entity, String> {
    let mut rng = ctx.rng();
    let world_size = ctx
        .db
        .config()
        .id()
        .find(&0)
        .ok_or("Config not found")?
        .world_size;
    let tile_size = ctx
        .db
        .config()
        .id()
        .find(&0)
        .ok_or("Config not found")?
        .tile_size;

    let x = rng.gen_range((-1.0 * tile_size as f32 * world_size as f32 / 2.0)..(tile_size as f32 * world_size as f32 / 2.0));
    let y = 1.0;
    let z = rng.gen_range((-1.0 * tile_size as f32 * world_size as f32 / 2.0)..(tile_size as f32 * world_size as f32 / 2.0));
    
    spawn_fisherman_at(
        ctx,
        player_id,
        DbVector3 { x, y, z },
    )
}

// SPAWN FISHERMAN AT LOCATION FUNCTION
fn spawn_fisherman_at(
    ctx: &ReducerContext,
    player_id: u32,
    position: DbVector3,
) -> Result<Entity, String> {
    let entity = ctx.db.entity().try_insert(Entity {
        entity_id: 0,
        position,
    })?;

    ctx.db.fisherman().try_insert(Fisherman {
        entity_id: entity.entity_id,
        player_id,
        direction: DbVector3 { x: 0.0, y: 0.0, z: 1.0 },
        speed: 0.0,
    })?;
    Ok(entity)
}
