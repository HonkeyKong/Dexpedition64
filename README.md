# Dexpedition64

This is a save manager for the Nintendo 64, meant to be like a swiss army knife for N64 saves. It supports the generally universal [MPK format](https://n64brew.dev/wiki/Controller_Pak/Filesystem) used by most emulators and devices like BlueRetro adapters, notes exported from [MPKEdit](https://bryc.github.io/mpkedit/) as well as the original old-school [DexDrive](https://en.wikipedia.org/wiki/DexDrive) saves you might find on [GameFAQs](https://gamefaqs.gamespot.com/n64), or an old floppy disk you forgot you had.

Speaking of the DexDrive, this is the main reason the software exists. I wanted a modern way to access saves with these old devices, but one didn't exist like they do for the PlayStation. So what do I do when I want something and it doesn't exist? I make it, of course! Hardware serial ports as well as USB to Serial converters have been tested and confirmed to work.

Right now the software supports formatting memory cards, as well as copying them and writing new images to them. You can also use the manager to delete, import and export individual save files, in the [MPKNote](https://github.com/bryc/mpkedit) format.

Once you've hit the "Browse Card" button, the software will read the entire contents of the card into RAM. From there, it works similar to a partition manager, where changes to the card are not saved until you press the "Write Card" button. This also means that copying one memory card to another is extremely easy, as you just need to browse one, then remove it, insert the card you want to write and press the "Write Card" button. 

From there, you can also use the "Save MPK" button to (obviously) save your card to MPK format. You can also press "New MPK" to create a new virtual memory card, which can be saved to file or written to a memory card.

Saving individual files is as easy as using the "Export Note" button, and inserting new files into the card can be done by pressing "Import Note". The last and hopefully obvious function is "Delete Note", which as I'm sure you guessed, deletes the currently selected note in the list.

I'm not really sure what else needs to be said about this, so I guess this is the part where I talk about the license. Licensed under GPLv3, as per the terms of [MemCardRex](https://github.com/ShendoXT/memcardrex), from which some of the DexDrive communication code was borrowed.