# Fuzzy Chess
Fuzzy Chess is a chess themed strategy game with many features that distinguish it from a typical game of chess. Some of the more notable features include corps (allowing for multiple moves each turn), fuzzy logic (randomizing the chance of capture based on piece power), and a variant chess ruleset (pieces have different movement rules when compared to typical chess). 

Created for an undergraduate Senior Project at KSU (Spring 2022) by Parker Smith (Team Lead), Matthew Graham, Cameron Jones, Adam Logan, Devin Pacl, Nicholas Wile, and Tyler Williams.

## Features
### Corps
The "armies" of pieces are divided into three corps, each of which may move once per turn:

The left bishop commands the three left pawns and the left knight.

The right bishop commands the three right pawns and the right knight.

The king commands the queen, the two rooks, and the remaining two center pawns.

The king may delegate any of its pieces to be commanded by either bishop, at any time. It may also revoke the delegation.

### Fuzzy Logic
Capturing pieces is not automatic. Capturing of pieces is rolled based on the attack strength and defense strength of certain pieces. A pawn is very easy to attack and cannot defend well. A rook has high defense and a moderate attack roll.

### Variant Ruleset
All pieces may move in non-linear directions, but pieces may not jump over other pieces (excluding the rook). Additionally, knights have a special sneak attack move where they can attack any square next to them after they have already made their move, adding +1 to the roll value for the sneak attack.

### AI
Pieces scan their local areas for opportunities or threats, and take action based on the best possible move. This is repeated until each corp has no moves left.
