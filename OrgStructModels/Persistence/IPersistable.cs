using System;
using System.ComponentModel;

namespace OrgStructModels.Persistence
{
    // interface for persistable business objects
    public interface IPersistable
    {
        /// <summary>
        /// The globally unique object identifier of the Persistable (Guid V4).
        /// </summary>
        Guid ObjectID { get; }

        /// <summary>
        /// Type name of this Persistable, as string. For developer's/admin's convenience only.
        /// </summary>
        string ObjectType { get; }

        /// <summary>
        /// Set to true by the Persistable when a property of this Persistable has changed.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// True indicates that this Persistable exists in persistent storage.
        /// </summary>
        bool IsPersistent { get; }

        /// <summary>
        /// Timestamp (UTC) of last change to this Persistable.
        /// </summary>
        DateTime ChangedAtUTC { set; get; }

        /// <summary>
        /// Marks the Persistable as dirty and raises PropertyChanged events.
        /// </summary>
        void Spoil();

        /// <summary>
        /// Creates a clone (exact copy, with the same objectID as the source) of this Persistable.
        /// </summary>
        /// <typeparam name="P">Persistable type derived from IPersitable.</typeparam>
        /// <returns>Clone of the Persistable.</returns>
        P Clone<P>() where P : IPersistable;

        /// <summary>
        /// Copies data from this Persistable into target Persistable.
        /// </summary>
        /// <typeparam name="P">Persistable type derived from IPersistable.</typeparam>
        /// <param name="target">The target Persistable to copy data into.</param>
        void CopyInto<P>(P target) where P : IPersistable;

        /// <summary>
        /// Copies data from source Persistable into this Persistable.
        /// </summary>
        /// <typeparam name="P">Persistable type derived from IPersistable.</typeparam>
        /// <param name="source">The source Persitable to copy data from.</param>
        void CopyFrom<P>(P source) where P : IPersistable;
    }
}
