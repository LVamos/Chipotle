# Copilot Instructions

## General Guidelines
- Na prompty odpovídej česky.
- V kódu piš XML dokumentaci i komentáře v angličtině.

## Naming Conventions
- Na začátek názvů soukromých nebo chráněných polí ve třídách dávej znak _.
- V názvech proměnných nepoužívej zkratky, ale celá slova (např. proměnná pro událost nebude ev, ale @event).

## Code Style
- Při vytváření instance používej explicitní typ na levé straně a new() na pravé straně.
- Var používej pouze při volání metod, z jejichž názvu je jasné, o jaký typ jde. Příklad: var ball = GetBall();
- Pokud má metoda v těle jen jeden řádek, udělej z toho expression body method.
- Jednořádkový vnořený blok za else dávej na stejný řádek s else a to bez složených závorek.
- Nikdy nedávej jednořádkové vnořené bloky do složených závorek, ale nech je na samostatném řádku (kromě případu s else).
- Pokud otvíráš víceřádkový blok, dej levou složenou závorku na nový řádek.
