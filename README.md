# Demo
[![Watch the video](http://i3.ytimg.com/vi/ah1NKIwMfgk/hqdefault.jpg)](https://www.youtube.com/watch?v=ah1NKIwMfgk)

# Features

- [x] Score bonus for greater combinations
- [ ] Gameplay
  - [x] Classic - simple match 3 game combined elements will be simply removed
  - [x] Accumulative - match 3 game but combined elements will be accumulated like in 2048
  - [ ] Mine down - match 3 game with some sort of "ground" that is mined by making combinations near it
  - [ ] Gravity changes - match 3 game with different direction of elements move(from up, from bottom, from left, from right)
  - [ ] Finite time - match 3 game where score is time that counts down, better move greater plus time
  - [ ] Collection mode - match 3 game where you need to move down treasures for collection
  - [ ] Ice mode - match 3 game where inactive for long time elements start freeze, to unfreeze them need to make combination nearby
  - [x] Sandbox - as much as possible customizable math 3 game
  - [ ] Different field shapes
    - [x] Classic rectangle
    - [x] With blockers inside
    - [x] Not rectangular shapes pressets
      - [x] Circle
  - [ ] Different cell shape
    - [x] Square
    - [ ] Hexagon
    - [ ] Octagon (seems feasible only for spherical field) ?
  - [x] Spawn and move scenarios
    - [x] Move then spawn inplace
    - [x] Spawn outside then move all
  - [x] Move directions
    - [x] Top to bottom
    - [x] Right to left
    - [x] Bottom to top
    - [x] Left to right
  - [ ] Move with obstacles
    - [x] Move behind obstacle
    - [ ] Bubble approach to empty cell
    - [ ] Obstacle as spawn point
- [ ] Abilities
  - [x] Movable abilities
    - [x] Hammer - Remove one selected element
    - [x] Bomb - removes zone 3x3
    - [x] Lightning bolt - removes all element with same value on field
    - [ ] Row&Column remove
  - [x] Static abilities
    - [x] Shuffle - shuffles board
    - [x] Generator upgrade - new elements range increase by 1
    - [x] Search - highlights one of the available moves
  - [ ] Abilities upgrades
    - [ ] Bomb shapes
  - [x] Abilities cooldowns
  - [ ] Activate ability by watching add
- [ ] Visual
  - [x] Dynamic game elements
    - [x] Shapes
      - [x] Regular polygon
      - [x] Star
      - [ ] Circle as some special rare element
    - [x] Shape variations
      - [x] Simple shape
      - [x] Shape with rounded corners
      - [x] Shape with rounded edges
        - [x] Convex edges
        - [x] Concave edges
    - [ ] Colors
        - [x] Simple pallet (now used "#3DFF53", "#FF4828", "#0008FF", "#14FFF3", "#FF05FA", "#FFFB28", "#FF6D0A", "#CB0032", "#00990A", "#990054")
        - [ ] Stripes pallet
        - [ ] Other patterns ?
  - [ ] Text particles for score changes
  - [ ] Combo visualization
  - [ ] UI
    - [x] Main menu
    - [x] Available game modes choice
    - [ ] Settings
    - [ ] About
    - [ ] Statistics
