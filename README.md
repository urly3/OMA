## o!ma

an app that pulls (_scrapes_, really) data from osu! multiplayer matches in order to generate some statistics
this is wrapped in the idea of an alias, which you can use to store a collection of multiplayer matches and see their results later
the reults themselves are generated with each call, as data may be subject to change

lobbies can be locked and unlocked - allowing you to 'archive' a collection of lobbies, for example a tournament or specific round of one

you can run a migration to get an sqlite database created and ready, otherwise, change the provided 'blank.db' to 'oma.db' (i keep my test one in the repo, cause it's mine - feel free to use it)

it uses sqlite for persistence, efcore for orm, and mustache/stubble for templating
i really enjoyed how simply this stack was to use, and look forward to diving into css to make it pretty (the site is only markup, as you can probably see)

i have some ideas that i may implement if i feel like it to make things a bit more useful

## running

```
git clone
cd oma
dotnet run
```

use osu world cup wiki for some matches you can use as an example
lobby id's are at the end of the multiplayer link provided with the wiki's results section