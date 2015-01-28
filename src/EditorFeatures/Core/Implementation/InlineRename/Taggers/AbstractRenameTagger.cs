﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Rename.ConflictEngine;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Text.Shared.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.Implementation.InlineRename
{
    internal abstract class AbstractRenameTagger<T> : ITagger<T>, IDisposable where T : ITag
    {
        private readonly ITextBuffer _buffer;
        private readonly InlineRenameService _renameService;

        private InlineRenameSession.OpenTextBufferManager _bufferManager;
        private IEnumerable<RenameTrackingSpan> _currentSpans;

        protected AbstractRenameTagger(ITextBuffer buffer, InlineRenameService renameService)
        {
            _buffer = buffer;
            _renameService = renameService;

            _renameService.ActiveSessionChanged += OnActiveSessionChanged;

            if (_renameService.ActiveSession != null)
            {
                AttachToSession(_renameService.ActiveSession);
            }
        }

        private void OnActiveSessionChanged(object sender, InlineRenameService.ActiveSessionChangedEventArgs e)
        {
            if (e.PreviousSession != null)
            {
                DetachFromSession(e.PreviousSession);
            }

            if (_renameService.ActiveSession != null)
            {
                AttachToSession(_renameService.ActiveSession);
            }
        }

        private void AttachToSession(InlineRenameSession session)
        {
            if (session.TryGetBufferManager(_buffer, out _bufferManager))
            {
                _bufferManager.SpansChanged += OnSpansChanged;
                OnSpansChanged();
            }
        }

        private void DetachFromSession(InlineRenameSession session)
        {
            if (_bufferManager != null)
            {
                RaiseTagsChangedForEntireBuffer();

                _bufferManager.SpansChanged -= OnSpansChanged;
                _bufferManager = null;
                _currentSpans = null;
            }
        }

        private void OnSpansChanged()
        {
            _currentSpans = _bufferManager.GetRenameTrackingSpans();
            RaiseTagsChangedForEntireBuffer();
        }

        private void RaiseTagsChangedForEntireBuffer()
        {
            var tagsChanged = TagsChanged;
            if (tagsChanged != null)
            {
                tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length)));
            }
        }

        public void Dispose()
        {
            _renameService.ActiveSessionChanged -= OnActiveSessionChanged;

            if (_renameService.ActiveSession != null)
            {
                DetachFromSession(_renameService.ActiveSession);
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<T>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (_renameService.ActiveSession == null)
            {
                yield break;
            }

            var renameSpans = _currentSpans;
            if (renameSpans != null)
            {
                var snapshot = spans.First().Snapshot;
                foreach (var renameSpan in renameSpans)
                {
                    var span = renameSpan.TrackingSpan.GetSpan(snapshot);
                    if (spans.OverlapsWith(span))
                    {
                        TagSpan<T> tagSpan;
                        if (TryCreateTagSpan(span, renameSpan.Type, out tagSpan))
                        {
                            yield return tagSpan;
                        }
                    }
                }
            }
        }

        protected abstract bool TryCreateTagSpan(SnapshotSpan span, RenameSpanKind type, out TagSpan<T> tagSpan);
    }
}