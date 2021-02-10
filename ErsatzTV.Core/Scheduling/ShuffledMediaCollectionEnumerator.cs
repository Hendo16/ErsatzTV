﻿using System;
using System.Collections.Generic;
using System.Linq;
using ErsatzTV.Core.Domain;
using ErsatzTV.Core.Interfaces.Scheduling;
using LanguageExt;
using static LanguageExt.Prelude;

namespace ErsatzTV.Core.Scheduling
{
    public class ShuffledMediaCollectionEnumerator : IMediaCollectionEnumerator
    {
        private readonly IList<MediaItem> _mediaItems;
        private Random _random;
        private IList<MediaItem> _shuffled;


        public ShuffledMediaCollectionEnumerator(IList<MediaItem> mediaItems, MediaCollectionEnumeratorState state)
        {
            _mediaItems = mediaItems;
            _random = new Random(state.Seed);
            _shuffled = Shuffle(_mediaItems, _random);

            State = new MediaCollectionEnumeratorState { Seed = state.Seed };
            while (State.Index < state.Index)
            {
                MoveNext();
            }
        }

        public MediaCollectionEnumeratorState State { get; }

        public Option<MediaItem> Current => _shuffled.Any() ? _shuffled[State.Index % _mediaItems.Count] : None;

        public void MoveNext()
        {
            State.Index++;
            if (State.Index % _shuffled.Count == 0)
            {
                State.Index = 0;
                State.Seed = _random.Next();
                _random = new Random(State.Seed);
                _shuffled = Shuffle(_mediaItems, _random);
            }

            State.Index %= _shuffled.Count;
        }

        private static IList<T> Shuffle<T>(IEnumerable<T> list, Random random)
        {
            T[] copy = list.ToArray();

            int n = copy.Length;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = copy[k];
                copy[k] = copy[n];
                copy[n] = value;
            }

            return copy;
        }
    }
}
