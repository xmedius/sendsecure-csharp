using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace XMedius.SendSecure.Helpers
{
    public interface IContact
    {}

    public class ContactInteraction<U> where U : IContact
    {
        public U ContactObject { get; set; }
        public Lazy<JObject> JObject { get; set; }

        public virtual string ToJson()
        {
            return JObject.Value.ToJson();
        }
    }

    public abstract class Creation<U> : ContactInteraction<U> where U : IContact
    {
        public abstract string EntryToRemove { get; }

        public override string ToJson()
        {
            var temp = JObject.Value;
            temp[typeof(U).Name.ToLower()].RemoveEntry(EntryToRemove)
                ["contact_methods"].RemoveEntries(new List<string> { "id", "_destroy" });
            return temp.ToJson();
        }
    }

    public class Edition<U> : ContactInteraction<U> where U : IContact
    {
        public override string ToJson()
        {
            var temp = JObject.Value;
            temp[typeof(U).Name.ToLower()]["contact_methods"].RemoveEntry("_destroy");
            return temp.ToJson();
        }
    }

    public abstract class Destruction<U> : ContactInteraction<U> where U : IContact
    {
        public abstract List<ContactMethod> ObjectContactMethods { get; }

        public Destruction<U> OfFollowingContacts(List<int?> contactMethodsIds)
        {
            ObjectContactMethods.ForEach(contact =>
            {
                if (contactMethodsIds.Contains(contact.Id))
                {
                    contact.DestroyContact = true;
                }
            });
            return this;
        }
    }
}
