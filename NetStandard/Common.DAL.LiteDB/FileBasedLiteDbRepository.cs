using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;

namespace Scar.Common.DAL.LiteDB
{
    public abstract class FileBasedLiteDbRepository<TId> : IFileBasedRepository
    {
        // direct bson types
        protected readonly HashSet<Type> BsonTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(int),
            typeof(long),
            typeof(bool),
            typeof(Guid),
            typeof(DateTime),
            typeof(byte[]),
            typeof(ObjectId),
            typeof(double),
            typeof(decimal)
        };

        protected readonly LiteDatabase Db;

        protected readonly Func<BsonValue, TId> FromBson;

        protected readonly Func<TId, BsonValue> ToBson;

        protected FileBasedLiteDbRepository(string directoryPath, string fileName, bool shrink = true)
        {
            DbFileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            DbDirectoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
            var idType = typeof(TId);

            // ReSharper disable once VirtualMemberCallInConstructor
            if (IsBson || BsonTypes.Contains(idType))
            {
                ToBson = id => new BsonValue(id);
                FromBson = bson => (TId)bson.RawValue;
            }
            else
            {
                ToBson = id => BsonMapper.Global.ToDocument(id);
                FromBson = bson => BsonMapper.Global.ToObject<TId>(bson.AsDocument);
            }

            if (!Directory.Exists(DbDirectoryPath))
            {
                Directory.CreateDirectory(DbDirectoryPath);
            }

            Db = new LiteDatabase(Path.Combine(DbDirectoryPath, $"{DbFileName}{DbFileExtension}"));
            if (shrink)
            {
                Db.Shrink();
            }
        }

        protected virtual bool IsBson => false;

        public string DbDirectoryPath { get; }

        public string DbFileExtension { get; } = ".db";

        public string DbFileName { get; }

        public void Dispose()
        {
            Db.Dispose();
        }

        protected virtual TId GenerateId()
        {
            return default!;
        }
    }
}