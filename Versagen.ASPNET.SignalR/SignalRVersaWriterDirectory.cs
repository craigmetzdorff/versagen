using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Versagen.IO;

namespace Versagen.ASPNET.SignalR
{

    public class SignalRWriterDirectoryBackingStore
    {
        public ConcurrentDictionary<VersaCommsID, ImmutableList<VersaCommsID>> groupItems { get; } =
            new ConcurrentDictionary<VersaCommsID, ImmutableList<VersaCommsID>>();

        public ConcurrentDictionary<VersaCommsID, ImmutableList<string>> ConnectionGroupings { get; } =
            new ConcurrentDictionary<VersaCommsID, ImmutableList<string>>();
    }


    public class SignalRWriterDirectory<THub> : IVersaWriterDirectory where THub:Hub
    {
        public IHubContext<THub> hubContext { get; }

        SignalRWriterDirectoryBackingStore Store { get; }

        VersaSignalRConfig config { get; }

        bool IsCustomGroupable(VersaCommsID id) => (!id.IdType.HasFlag(EVersaCommIDType.Unicast));

        public Task AddSingleEntityConnection(VersaCommsID destinationID, string connectionID)
        {
            if (Store.ConnectionGroupings.GetOrAdd(destinationID, ImmutableList.Create<string>()).Contains(connectionID)
                ) return Task.CompletedTask;

            Store.ConnectionGroupings.AddOrUpdate(destinationID, ImmutableList.Create<string>().Add(connectionID),
                (k, l) => l.Add(connectionID));

            var AddTasks = new List<Task>
            {
                hubContext.Groups.AddToGroupAsync(connectionID, "versagen_" + destinationID)
            };
            foreach (var item in Store.groupItems)
                if (item.Value.Contains(destinationID))
                    AddTasks.Add(hubContext.Groups.AddToGroupAsync(connectionID, "versagen_" + item.Key));
            return Task.WhenAll(AddTasks);
        }

        public Task AddToSignalRConnection(VersaCommsID groupID, VersaCommsID connectionGroupingID)
        {
            if (!Store.ConnectionGroupings.ContainsKey(connectionGroupingID)) return Task.CompletedTask;
            if (!Store.groupItems.ContainsKey(groupID))
                Store.groupItems.AddOrUpdate(groupID, ImmutableList.Create<VersaCommsID>().Add(connectionGroupingID), (k, l) => l.Add(connectionGroupingID));

            return Task.WhenAll(Store.ConnectionGroupings[connectionGroupingID]
                .Select(c => hubContext.Groups.AddToGroupAsync(c, "versagen_" + groupID)));
        }

        public Task RemoveFromSignalRConnection(VersaCommsID groupID, VersaCommsID connectionGroupingID)
        {
            if (!Store.ConnectionGroupings.ContainsKey(connectionGroupingID)) return Task.CompletedTask;
            if (Store.groupItems.ContainsKey(groupID))
                Store.groupItems.AddOrUpdate(groupID, ImmutableList.Create<VersaCommsID>(), (k, l) => l.Remove(connectionGroupingID));
            return Task.WhenAll(Store.ConnectionGroupings[connectionGroupingID]
                .Select(c => hubContext.Groups.RemoveFromGroupAsync(c, "versagen_" + groupID)));
        }

        HtmlEncoder encoder;

        public void Dispose()
        {
            //
        }

        public IVersaGroupWriter GetOrCreateGroup(VersaCommsID id)
        {
            if (!Store.groupItems.ContainsKey(id))
                Store.groupItems.AddOrUpdate(id, ImmutableList.Create<VersaCommsID>(), (k, l) => l);
            return new SignalRVersaGroupWriter<THub>(id, this,
                hubContext.Clients.Group("versagen_" + id), config.NameOfWriterFunction, encoder);
            
        }

        public IVersaGroupWriter CreateGroup(EVersaCommIDType type)
        {
            return GetOrCreateGroup(VersaCommsID.RandomOutsideRange(type, Store.groupItems.Keys));
        }

        public bool DeleteGroupWriter(VersaCommsID id)
        {
            if (!Store.groupItems.ContainsKey(id)) return false;
            foreach (var x in Store.groupItems[id])
            {
                RemoveFromSignalRConnection(id, x).Wait();
            }
            Store.groupItems.Remove(id, out _);
            return true;
        }

        public bool DeleteGroupWriter(IVersaGroupWriter writer) => DeleteGroupWriter(writer.DestinationID);

        public IVersaWriter GetWriter(VersaCommsID Id)
        {
            //Ignoring users method b/c generic usage. Someone might not be using Identity with this so it may be a different user that is contacted.
            return new VersaSignalRWriter(Id, hubContext.Clients.Group("versagen_" + Id), config.NameOfWriterFunction,
                encoder);
        }

        //TODO: really needed?
        public IQueryable<IVersaWriter> Writers { get; }

        public SignalRWriterDirectory(IHubContext<THub> context, SignalRWriterDirectoryBackingStore backingStore, HtmlEncoder encoder, VersaSignalRConfig config)
        {
            hubContext = context;
            Store = backingStore;
            this.encoder = encoder;
            this.config = config;
        }
    }
}
