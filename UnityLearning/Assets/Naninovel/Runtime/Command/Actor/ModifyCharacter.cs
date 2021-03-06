﻿// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Modifies a [character actor](/guide/characters.md).
    /// </summary>
    /// <example>
    /// ; Shows character with ID `Sora` with a default appearance.
    /// @char Sora
    /// 
    /// ; Same as above, but sets appearance to `Happy`.
    /// @char Sora.Happy
    /// 
    /// ; Same as above, but also positions the character 45% away from the left border 
    /// ; of the screen and 10% away from the bottom border; also makes him look to the left.
    /// @char Sora.Happy look:left pos:45,10
    /// 
    /// ; Make Sora appear at the bottom-center and in front of Felix
    /// @char Sora pos:50,0,-1
    /// @char Felix pos:,,0
    /// </example>
    [CommandAlias("char")]
    public class ModifyCharacter : ModifyOrthoActor<ICharacterActor, CharacterState, CharacterMetadata, CharactersConfiguration, CharacterManager>
    {
        /// <summary>
        /// ID of the character to modify and the appearance to set.
        /// When appearance is not provided, will use either a `Default` (is exists) or a random one.
        /// </summary>
        [CommandParameter(NamelessParameterAlias)]
        public Named<string> IdAndAppearance { get => GetDynamicParameter<Named<string>>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Look direction of the actor; possible options: left, right, center.
        /// </summary>
        [CommandParameter("look", true)]
        public string LookDirection { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Name (path) of the [avatar texture](/guide/characters.md#avatar-textures) to assign for the character.
        /// Use `none` to remove (un-assign) avatar texture from the character.
        /// </summary>
        [CommandParameter("avatar", true)]
        public string AvatarTexturePath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        [CommandParameter(optional: true)] // Allows specifying ID via the nameless parameter.
        public override string Id { get => IdAndAppearance.Item1; set => base.Id = value; }
        [CommandParameter(optional: true)] // Allows specifying appearance via the nameless parameter.
        public override string Appearance { get => IdAndAppearance.Item2; set => base.Appearance = value; }

        private bool autoArrange;

        public override async Task ExecuteAsync (CancellationToken cancellationToken = default)
        {
            await base.ExecuteAsync(cancellationToken);

            if (AvatarTexturePath is null) // Check if we can map current appearance to an avatar texture path.
            {
                var avatarPath = $"{Id}/{Appearance}";
                if (ActorManager.AvatarTextureExists(avatarPath) && ActorManager.GetAvatarTexturePathFor(Id) != avatarPath)
                    ActorManager.SetAvatarTexturePathFor(Id, avatarPath);
                else // Check if a default avatar texture for the character exists and assign if it does.
                {
                    var defaultAvatarPath = $"{Id}/Default";
                    if (ActorManager.AvatarTextureExists(defaultAvatarPath) && ActorManager.GetAvatarTexturePathFor(Id) != defaultAvatarPath)
                        ActorManager.SetAvatarTexturePathFor(Id, defaultAvatarPath);
                }
            }
            else // User provided specific avatar texture path, assigning it.
            {
                if (AvatarTexturePath.EqualsFastIgnoreCase("none"))
                    ActorManager.RemoveAvatarTextureFor(Id);
                else ActorManager.SetAvatarTexturePathFor(Id, AvatarTexturePath);
            }
        }

        protected override async Task ApplyModificationsAsync (ICharacterActor actor, EasingType easingType, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            tasks.Add(base.ApplyModificationsAsync(actor, easingType, cancellationToken));
            tasks.Add(ApplyLookDirectionModificationAsync(actor, easingType, cancellationToken));

            if (autoArrange)
                tasks.Add(ActorManager.ArrangeCharactersAsync(Duration, easingType, cancellationToken));

            await Task.WhenAll(tasks);
        }

        protected virtual async Task ApplyLookDirectionModificationAsync (ICharacterActor actor, EasingType easingType, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(LookDirection)) return;
            if (LookDirection.EqualsFastIgnoreCase("right")) await actor.ChangeLookDirectionAsync(CharacterLookDirection.Right, Duration, easingType, cancellationToken);
            else if (LookDirection.EqualsFastIgnoreCase("left")) await actor.ChangeLookDirectionAsync(CharacterLookDirection.Left, Duration, easingType, cancellationToken);
            else if (LookDirection.EqualsFastIgnoreCase("center")) await actor.ChangeLookDirectionAsync(CharacterLookDirection.Center, Duration, easingType, cancellationToken);
            else { Debug.LogError("Unsupported value for LookDirection."); return; }
        }

        protected override Task ApplyAppearanceModificationAsync (ICharacterActor actor, EasingType easingType, CancellationToken cancellationToken)
        {
            // When adding character on scene, change appearance instantly to prevent bleeding previous one during fade-in.
            if (!string.IsNullOrEmpty(Appearance) && !actor.IsVisible && Visible.HasValue && Visible.Value)
            {
                actor.Appearance = Appearance;
                return Task.CompletedTask;
            }

            return base.ApplyAppearanceModificationAsync(actor, easingType, cancellationToken);
        }

        protected override async Task ApplyVisibilityModificationAsync (ICharacterActor actor, EasingType easingType, CancellationToken cancellationToken)
        {
            // Execute auto-arrange if adding to scene, no position specified and auto-arrange on add is enabled.
            if (ScenePosition is null && !actor.IsVisible && Visible.HasValue && Visible.Value && ActorManager.AutoArrangeOnAdd)
                autoArrange = true;

            await base.ApplyVisibilityModificationAsync(actor, easingType, cancellationToken);
        }
    } 
}
