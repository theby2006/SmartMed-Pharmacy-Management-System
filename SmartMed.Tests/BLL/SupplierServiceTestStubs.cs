using System;
using System.Collections.Generic;
using SmartMed.DAL.Interfaces;
using SmartMed.Models.Entities;

namespace SmartMed.Tests.BLL
{
    internal class MockSupplierRepository : ISupplierRepository
    {
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
        public List<int> SuppliersWithPurchases { get; set; } = new List<int>();
        public bool AddCalled { get; set; }
        public bool UpdateCalled { get; set; }
        public bool DeleteCalled { get; set; }

        public List<Supplier> GetAll() => Suppliers;

        public Supplier GetById(int id) =>
            Suppliers.Find(s => s.Id == id);

        public Supplier GetBySupplierCode(string supplierCode) =>
            Suppliers.Find(s =>
                s.SupplierCode.Equals(supplierCode, StringComparison.OrdinalIgnoreCase));

        public Supplier GetByName(string supplierName) =>
            Suppliers.Find(s =>
                s.SupplierName.Equals(supplierName, StringComparison.OrdinalIgnoreCase));

        public List<Supplier> Search(string keyword) =>
            Suppliers.FindAll(s =>
                s.SupplierCode.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                s.SupplierName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (s.CompanyName != null && s.CompanyName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (s.PhoneNumber != null && s.PhoneNumber.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0));

        public int Add(Supplier supplier)
        {
            AddCalled = true;
            supplier.Id = Suppliers.Count + 1;
            Suppliers.Add(supplier);
            return supplier.Id;
        }

        public void Update(Supplier supplier)
        {
            UpdateCalled = true;
            int index = Suppliers.FindIndex(s => s.Id == supplier.Id);
            if (index >= 0) Suppliers[index] = supplier;
        }

        public void Delete(int id)
        {
            DeleteCalled = true;
            Suppliers.RemoveAll(s => s.Id == id);
        }

        public bool HasPurchases(int supplierId) =>
            SuppliersWithPurchases.Contains(supplierId);
    }

}
