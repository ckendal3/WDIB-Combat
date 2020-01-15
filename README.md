# WDIBCombat
Multi-threaded combat system created by Christian Kendall (GingerKendall) @WDIBStudios.

## Prologue
Thanks for taking a look at this! 
The ultimate goal is to make this a complete package for combat that is only component-based and very universal.
The original plan was to make this an asset on Unity to make so money but I don't feel like doing that anymore.
So! I use this across multiple projects (notably: Installation 01 (has quite a few changes)) and all my personal projects on Unity.
Please let me know if there are issues or if you have suggestions!

## Things to Know
Projectiles and explosives are currently viable to use. Weapons should be by mid-February.

There are trello boards to follow:
[Weapons Trello](https://trello.com/b/bDXbCzIO/ecsweapons "WDIB Weapon Trello")
[Projectiles Trello](https://trello.com/b/FguxaMwc/wdibprojectiles "WDIB Projectile Trello")


### TODO List (Not all listed)
1. Integrate default physics and new physics system using project-wide settings
2. Explosives need to check if objects within range are safe
3. Write a pooling system until ECS has VFX support
4. Write property drawer for IComponentData
5. Make SetupCommands for both Hit and MultiHit system (move to own class)
6. Components/System (a lot planned)
   * Zooming
   * Timed Reload
   * Network/Local inputs
   * Actioning for weapons
   * Impulse from projectiles/explosions (grenade jumping, ammiright?)
   * Implement visuals system (might be overscope)
   * Battery cooldown
   * Ammo reduction system
   * Weapon Offset system
7. Implement Overridable components/systems
8. Implement smart system for low entity count systems to use mainthread (or wait for Unity)
9. Implement Single, Burst, Automatic firing modes for weapons
10. Implement batched raycasts
11. Combined trello boards
12. Remove NaughtyAttributes (a nice package but need to be independent)
13. Implement Raycast ("hitscan") option for projectiles


### Known Issues (Not all listed)
1. Explosives can hit through walls.
  * Needs to have raycast hit checks to each rigidbody in range
2. Weapons are currently incomplete
3. Instancing (rendering) is incompatible with all render pipelines except HRDP due to ECS Rendermesh.
4. Fix 48.2 KB GC per frame in HitHandlerSystem (line 117 [Master Chief much])
5. Fix 21.9 KB GC per frame in ExplosiveSystem (line 80) 
6. Reduce main thread waiting across the board
7. Currently use damage as force in HitHandlerSystem (line 84)
8. MultiHitSystem currently eats the most performance, need to reduce it further
9. Currently destroying entities inside of HitHandlerSystem 
10. DestroyExplosiveSystem does not currently destroy the explosions
