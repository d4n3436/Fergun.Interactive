# Fergun.Interactive
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE) [![NuGet](https://img.shields.io/nuget/vpre/Fergun.Interactive)](https://www.nuget.org/packages/Fergun.Interactive) [![Discord](https://discord.com/api/guilds/460627183501574144/widget.png)](https://discord.gg/V3TgaZRUPX)

**Fergun.Interactive** is an addon that provides interactive functionality to Discord commands.

This is a fork of [Discord.InteractivityAddon](https://github.com/Playwo/Discord.InteractivityAddon) that adds several features, including more customization and support for interactions (buttons and select menus).

## Features

- Methods for sending and deleting a message after a timeout
- Methods for receiving incoming messages, reactions, or interactions
- Customizable paginator:
  - Uses pages that can be navigated through reactions or buttons
  - Supports button customization (emote, label, style, etc.)
  - Includes two types of paginators: static and lazy-loaded
  - Supports restricting usage to specific users
  - Provides canceled and timeout pages
  - Supports timeout and cancellation via a special option or cancellation token
  - Supports actions that are executed when a paginator stops, such as modifying/deleting the message or removing/disabling the reactions/components
  - Supports extension methods that can be used in any paginator builder
  - Supports custom paginators, inheriting from the `Paginator` and `PaginatorBuilder` classes
  - Allows jumping (skipping) to a specific page using message input or modals (more info [here](https://github.com/d4n3436/Fergun.Interactive/releases/tag/v1.5))

- 🆕 Component paginator:
  - A new type of paginator separate from the regular ones, written from scratch with customization and flexibility in mind, with support for components V2
  - Information about its usage can be found [here](../../wiki/Component-paginators).

- Customizable selection:
  - Uses a list of options for users to select from
  - Supports messages, reactions, buttons, and select menus
  - Supports restricting usage to specific users
  - Provides success, canceled, and timeout pages
  - Supports timeout and cancellation via a special option or cancellation token
  - Supports actions that are executed when a selection stops, such as modifying/deleting the message or removing/disabling the reactions/components
  - Supports extension methods that can be used in any selection builder
  - Fully generic, supports any type of option (by providing a string/emote converter for that type)
  - Supports custom selections, inheriting from the `BaseSelection` and `BaseSelectionBuilder` classes

## Usage
- Install via [NuGet](https://www.nuget.org/packages/Fergun.Interactive)
  
- Add the `InteractiveService` into your service provider:
```cs
using Fergun.Interactive;
...

var provider = new ServiceCollection()
    .AddSingleton(new InteractiveConfig { DefaultTimeout = TimeSpan.FromMinutes(5) }) // Optional config
    .AddSingleton<InteractiveService>()
    ...
```
- Inject the service via DI (constructor/property injection).

## Examples

The [Example Bot](ExampleBot) contains multiple examples with comments. The modules are registered as slash commands.

Example modules:
- [Wait for socket entities](ExampleBot/Modules/WaitModule.cs) (messages, reactions, etc.)

- [Selection](ExampleBot/Modules/SelectionModule.cs)

- [Pagination](ExampleBot/Modules/PaginatorModule.cs)

- Customization
  - [Selection with custom button colors](ExampleBot/Modules/CustomButtonModule.cs#L16) (`!custom button`)
  - [Multi selection](ExampleBot/Modules/CustomSelectModule.cs#L26) (`!custom select`) (Selection message with multiple select menus)
  - [Extension methods in builders](ExampleBot/Modules/CustomExtensionModule.cs#L17) (`!custom extension`)

## Q&A

### Q: Why the paginator/selection doesn't do anything after I press a reaction/button? / I'm getting an "A MessageReceived handler is blocking the gateway task." message in the console
A: You're blocking the gateway task with a method from the interactive service. Make sure your command is running in a different thread using `RunMode.Async`:

```cs
[Command("command", RunMode = RunMode.Async)]
public async Task Command()
...
```

### Q: Why is my reaction/message to the paginator/selection not automatically deleted even if I specified to delete valid or invalid responses?

A: The bot doesn't have the `ManageMessages` permission in the channel you're using the paginator/selection. This is required to delete messages and reactions.

### Q: When responding an interaction with a paginator/selection, Why does the response message have no components even if I specified to use buttons or select menus?

  - A: Your paginator only has one page. The library doesn't include components in this case.
  - A: You're not passing the correct response type. The default value is `ChannelMessageWithSource`, but if you've deferred the interaction, you'll have to use either `DeferredChannelMessageWithSource` (send a message) or `DeferredUpdateMessage` (update a message).

### Q: How can I get the last interaction of a paginator/selection to use it elsewhere, eg. send a modal?

A: You can get the interaction that stopped the paginator/selection through the `StopInteraction` property in `InteractiveMessageResult`.

Note that this interaction will already be deferred by default (if nothing else has already been done to the interaction before, like update the message).

You can prevent the library from doing this by setting the following options in `InteractiveConfig` to `false`:

```cs
var collection = new ServiceCollection()
    ...
    .AddSingleton(new InteractiveConfig
    {
        DeferStopPaginatorInteractions = false,
        DeferStopSelectionInteractions = false
    })
    .AddSingleton<InteractiveService>()
    ...
```

## Additions/Changes from Discord.InteractivityAddon (outdated)

 - Paginators now support buttons.
 - Selections now support buttons and select menus.
 - Merged `MessageSelection` and `ReactionSelection` into `Selection`.
 - Added `EmoteConverter` and `StringConverter` to `SelectionBuilder`. These properties are used to properly convert the generic options in the selection into the options that can be used to receive the incoming inputs, like messages (from `StringConverter`), reactions (from `EmoteConverter`), buttons and select menus (emotes and labels)
 - Added `EqualityComparer`to `SelectionBuilder`. This is used to determine there are no duplicate options.
 - Added `EmoteSelectionBuilder` and `EmoteSelectionBuilder<TValue>`, they are a variant of `SelectionBuilder` that uses emotes as input and provides overriden properties with default values, making them ready to use in selections using reactions and buttons.
 - Added `InteractiveStatus` to `InteractiveResult`, containing all the possible status of an interactive result.
 - In `PaginatorBuilder`, now `Emotes`, `WithEmotes()` and `AddEmote()` are named `Options`, `WithOptions()` and `AddOption()`, respectively.
 - In the methods that waits for a socket entity (`NextMessageAsync`, `NextReactionAsync`, etc.), now the `bool` parameter in `action` returns whether the entity *passed* the filter (the previous behavior was the same but inverted, not sure if this was intended).
 - Now the paginator/selection builders implement the fluent builder pattern using recursive generics. This makes creating custom builders much easier.
 - Now multiple input types (messages, reactions, buttons, etc.) can be used in a single paginator/selection.
 - Now `SendPaginatorAsync()` and `SendSelectionAsync()` returns an `InteractiveMessageResult`. This is the same as `InteractiveResult` but contains the message that has the paginator/selection.
 - Added a `messageAction` parameter to `SendPaginatorAsync()` and `SendSelectionAsync()`. This allows to execute any action after the message containing the paginator/selection is sent or modified.
 - Now the paginators don't reset their internal timeout timer when a valid input is received. This option can be enabled again using the `resetTimeoutOnInput` parameter.
 - Now `SendPaginatorAsync()` only sends a message and returns when a paginator with a single page is passed.
 - When using paginators/selections with reactions, now the process of adding the initial reactions will be canceled if the paginator/selection is canceled.
 - Added more post-execution actions to paginators and selections. Now it's possible to tell the paginator/selection to do the following after a cancellation/timeout (or a valid input in case of selections):
   - `DeleteInput` (remove the reactions/buttons/select menu from the message)
   - `DisableInput` (disable the buttons/select menu from the message)
   
   Plus the previously existing options:
    - `ModifyMessage` (Modifies the message to the Cancelled/Timeout/Success page)
    - `DeleteMessage`
   
   These options can also be combined, so you can modify the message *and* delete/disable the input.
   Note that using the option `DeleteMessage` will override any other option, and `DeleteInput`/`DisableInput` can't be used at the same time (for obvious reasons).
 - Now the incoming inputs (messages, reactions, interactions) are handled via callbacks. These callbacks are stored in a dictionary. This eliminates the need to subscribe to a local event handler each time an input is received, since now they are received in a single event handler, and also allows to cancel/remove/dispose any callback.
 - The following methods now wait for completion:
   - `DelayedSendMessageAndDeleteAsync()`
   - `DelayedDeleteMessageAsync()`
   - `DelayedSendFileAndDeleteAsync()`
   
   If you don't want to wait for completion, simply discard the Task:

   ```cs
   _ = Interactive.DelayedSendMessageAndDeleteAsync(...);
   ```
