using System;
using System.Collections.Generic;
using System.Data;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.Tests.BLL
{
    internal class MockMedicineCategoryRepository : IMedicineCategoryRepository
    {
        public List<MedicineCategory> Categories { get; set; } = new List<MedicineCategory>();
        public bool AddCalled { get; set; }
        public bool UpdateCalled { get; set; }
        public bool DeleteCalled { get; set; }

        public List<MedicineCategory> GetAll() => Categories;

        public MedicineCategory GetById(int id) =>
            Categories.Find(c => c.Id == id);

        public MedicineCategory GetByName(string name) =>
            Categories.Find(c => c.Name == name);

        public int Add(MedicineCategory category)
        {
            AddCalled = true;
            category.Id = Categories.Count + 1;
            Categories.Add(category);
            return category.Id;
        }

        public void Update(MedicineCategory category)
        {
            UpdateCalled = true;
            int index = Categories.FindIndex(c => c.Id == category.Id);
            if (index >= 0) Categories[index] = category;
        }

        public void Delete(int id)
        {
            DeleteCalled = true;
            Categories.RemoveAll(c => c.Id == id);
        }
    }

    internal class MockMedicineRepository : IMedicineRepository
    {
        public List<Medicine> Medicines { get; set; } = new List<Medicine>();
        public bool AddCalled { get; set; }
        public bool UpdateCalled { get; set; }
        public bool DeleteCalled { get; set; }

        public Medicine GetById(int id) =>
            Medicines.Find(m => m.Id == id);

        public List<Medicine> GetAll() => Medicines;

        public List<Medicine> GetByCategoryId(int categoryId) =>
            Medicines.FindAll(m => m.CategoryId == categoryId);

        public List<Medicine> Search(string keyword) =>
            Medicines.FindAll(m =>
                m.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (m.Brand != null && m.Brand.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0));

        public List<Medicine> GetLowStock() =>
            Medicines.FindAll(m => m.StockQuantity <= m.ReorderLevel);

        public List<Medicine> GetNearExpiry(int thresholdDays) =>
            Medicines.FindAll(m => m.ExpiryDate.HasValue &&
                m.ExpiryDate.Value <= DateTime.UtcNow.AddDays(thresholdDays) &&
                m.ExpiryDate.Value >= DateTime.UtcNow);

        public Medicine GetByNameAndBrand(string name, string brand) =>
            Medicines.Find(m =>
                m.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                ((m.Brand == null && brand == null) ||
                 (m.Brand != null && m.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase))));

        public int Add(Medicine medicine)
        {
            AddCalled = true;
            medicine.Id = Medicines.Count + 1;
            Medicines.Add(medicine);
            return medicine.Id;
        }

        public void Update(Medicine medicine)
        {
            UpdateCalled = true;
            int index = Medicines.FindIndex(m => m.Id == medicine.Id);
            if (index >= 0) Medicines[index] = medicine;
        }

        public void Delete(int id)
        {
            DeleteCalled = true;
            Medicines.RemoveAll(m => m.Id == id);
        }

        public void SetStockQuantity(int medicineId, int quantity)
        {
            Medicine medicine = Medicines.Find(m => m.Id == medicineId);
            if (medicine != null) medicine.StockQuantity = quantity;
        }

        public void SetStockQuantity(int medicineId, int quantity, IDbConnection connection, IDbTransaction transaction)
        {
            SetStockQuantity(medicineId, quantity);
        }

        public void UpdateStockQuantity(int medicineId, int delta)
        {
            Medicine medicine = Medicines.Find(m => m.Id == medicineId);
            if (medicine != null) medicine.StockQuantity += delta;
        }

        public void UpdateStockQuantity(int medicineId, int delta, IDbConnection connection, IDbTransaction transaction)
        {
            UpdateStockQuantity(medicineId, delta);
        }
    }
}
