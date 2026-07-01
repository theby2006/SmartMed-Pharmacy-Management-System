using System;
using System.Data;
using System.Data.SqlClient;
using SmartMed.Common.Exceptions;
using SmartMed.Common.Helpers;
using SmartMed.DAL.Interfaces;

namespace SmartMed.DAL.Infrastructure
{
    public class SqlUnitOfWork : IUnitOfWork
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private bool _disposed;

        public SqlUnitOfWork(IDbConnectionFactory connectionFactory)
        {
            Guard.AgainstNull(connectionFactory, nameof(connectionFactory));
            _connectionFactory = connectionFactory;
        }

        public IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = _connectionFactory.CreateConnection();
                }
                return _connection;
            }
        }

        public IDbTransaction Transaction => _transaction;

        public void BeginTransaction()
        {
            if (_connection == null)
            {
                _connection = _connectionFactory.CreateConnection();
            }

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            if (_transaction == null)
            {
                throw new DataAccessException("No transaction in progress. Call BeginTransaction first.");
            }

            try
            {
                _transaction.Commit();
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to commit transaction.", exception);
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Rollback()
        {
            if (_transaction == null)
            {
                return;
            }

            try
            {
                _transaction.Rollback();
            }
            catch (SqlException exception)
            {
                throw new DataAccessException("Failed to rollback transaction.", exception);
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }

                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }

            _disposed = true;
        }
    }
}
