# Fergun.Interactive
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE) [![NuGet](https://img.shields.io/nuget/vpre/Fergun.Interactive)](https://www.nuget.org/packages/Fergun.Interactive) [![Nuget](https://img.shields.io/nuget/vpre/Fergun.Interactive.Labs?label=nuget%20%28D.Net%20Labs%29)](https://www.nuget.org/packages/Fergun.Interactive.Labs)

Fergun.Interactive is an addon that adds interactive actions to commands.

This is a fork of [Discord.InteractivityAddon](https://github.com/Playwo/Discord.InteractivityAddon) that adds several features, including more customization and support for interactions (buttons and select menus).

## Usage
- Install via NuGet:
  - [Fergun.Interactive](https://www.nuget.org/packages/Fergun.Interactive) (For Discord.Net)
  - [Fergun.Interactive.Labs](https://www.nuget.org/packages/Fergun.Interactive.Labs) (For Discord.Net.Labs, supports buttons and select menus)
  
  Note: Use the preview version if you're using a preview version of Discord.Net(.Labs)
  
- Add the `InteractiveService` into your service provider:
```cs
using Fergun.Interactive;
...

var provider = new ServiceCollection()
    .AddSingleton<InteractiveService>()
    ...
```
- Inject the service via DI (constructor/property injection).

## Examples

The [ExampleBot](ExampleBot) contains multiple examples with comments. The default prefix is `!`.

Compile with the `DebugLabs` or `ReleaseLabs` configuration to be able to use interactions.

Example modules:
- Waiting for socket entities (messages, reactions, etc.)
  - WIP
- Selection
  - [Simple selection message](ExampleBot/Modules/SelectionModule.cs#L24) (`!select`)
  - [Emote selection message](ExampleBot/Modules/SelectionModule.cs#L64) (`!select emote`) (for selections using reactions/buttons as input)
  - [Emote selection message 2](ExampleBot/Modules/SelectionModule.cs#L98) (`!select emote2`)
  - [Selection message with extra features](ExampleBot/Modules/SelectionModule.cs#L135) (`!select extra`)
  - [Menu](ExampleBot/Modules/SelectionModule.cs#L193) (`!select menu`) (How to reuse a selection message)

- Paginator
  - WIP


## Additions/Changes from Discord.InteractivityAddon

 - Paginators now support buttons.
 - Selections now support buttons and select menus.
 - Merged `MessageSelection` and `ReactionSelection` into `Selection`.
 - Added `EmoteConverter` and `StringConverter` to `SelectionBuilder`. These properties are used to properly convert the generic options in the selection into the options that can be used to receive the incoming inputs, like messages (from `StringConverter`) and reactions (from `EmoteConverter`), buttons and select menus (emotes and labels)
 - Added `EqualityComparer`to `SelectionBuilder`. This is used to determine there are no duplicate options.
 - Added `EmoteSelectionBuilder` and `EmoteSelectionBuilder<TValue>`, they are a variant of `SelectionBuilder` that uses emotes as input and provides overriden properties with default values, making them ready to use in selections using reactions and buttons.
 - Added `InteractiveStatus` to `InteractiveResult`, containing all the possible status of an interactive result.
 - In `PaginatorBuilder`, now `Emotes`, `WithEmotes()` and `AddEmote()` are named `Options`, `WithOptions()` and `AddOption()`, respectively.
 - In the methods that waits for a socket entity (`NextMessageAsync`, `NextReactionAsync`, etc.), now the `bool` parameter in `action` returns whether the entity *passed* the filter (the previous behavior was the same but inverted, not sure if this was intended).
 - Now the paginator/selection builders implement the fluent builder pattern using recursive generics. This makes creating custom builders much easier.
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
