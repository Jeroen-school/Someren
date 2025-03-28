﻿using Someren.Models;
namespace Someren.Repositories
{
    public interface IRoomRepository
    {
        List<Room> GetAll();
        List<Room> GetBySize(int size);
        Room? GetById(int roomId);
        bool Add(Room room, out string errorMessage);
        bool Update(Room room, out string errorMessage);
        void Delete(int roomId);
    }
}
