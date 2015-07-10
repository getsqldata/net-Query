﻿namespace QckQuery
{
    using QckQuery.DataAccess;
    using QckQuery.Exception.Querying;
    using QckQuery.Formatting;
    using System.Transactions;

    public partial class QuickQuery
    {
        /// <summary>
        /// Runs the given query giving no return.
        /// It uses DbCommand.ExecuteNonQuery underneath. 
        /// </summary>
        /// <param name="sql">Query to run</param>
        /// <param name="parameters">Parameters names and values pairs</param>
        public void WithoutReturn(string sql, params object[] parameters)
        {
            using (var connection = _connectionProvider.GetOpenConnection())
            using (var command = connection.GetCommandWithParametersSet(sql, parameters.ToParameterDictionary()))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Runs the given query giving no return.
        /// Throws UnexpectedNumberOfRowsAffected if none or more than one row was affected.
        /// It uses DbCommand.ExecuteNonQuery underneath.
        /// </summary>
        /// <param name="sql">Query to run</param>
        /// <param name="parameters">Parameters names and values pairs</param>
        /// <exception cref="UnexpectedNumberOfRowsAffected">If none or more than one row was affected</exception>
        public void WithoutReturnAffectingExactlyOneRow(string sql, params object[] parameters)
        {
            WithoutReturnAffectingExactlyNRows(sql, 1, parameters);
        }

        /// <summary>
        /// Runs the given query giving no return.
        /// Throws UnexpectedNumberOfRowsAffected if more than one row was affected.
        /// It uses DbCommand.ExecuteNonQuery underneath.
        /// </summary>
        /// <param name="sql">Query to run</param>
        /// <param name="parameters">Parameters names and values pairs</param>
        /// <exception cref="UnexpectedNumberOfRowsAffected">If more than one row was affected</exception>
        public void WithoutReturnAffectingOneRowOrLess(string sql, params object[] parameters)
        {
            WithoutReturnAffectingNRowsOrLess(sql, 1, parameters);
        }

        /// <summary>
        /// Runs the given query giving no return.
        /// Throws UnexpectedNumberOfRowsAffected if the number of affected rows is different from N.
        /// It uses DbCommand.ExecuteNonQuery underneath.
        /// </summary>
        /// <param name="sql">Query to run</param>
        /// <param name="n">Number of affected rows to ensure</param>
        /// <param name="parameters">Parameters names and values pairs</param>
        /// <exception cref="UnexpectedNumberOfRowsAffected">
        /// If the number of affected rows is different from N
        /// </exception>
        public void WithoutReturnAffectingExactlyNRows(string sql, int n, params object[] parameters)
        {
            WithoutReturnAffectingNRows(sql, n, false, parameters);
        }

        /// <summary>
        /// Runs the given query giving no return.
        /// Throws UnexpectedNumberOfRowsAffected if the number of affected rows is greater than N.
        /// It uses DbCommand.ExecuteNonQuery underneath.
        /// </summary>
        /// <param name="sql">Query to run</param>
        /// <param name="n">Maximum of affected rows to ensure</param>
        /// <param name="parameters">Parameters names and values pairs</param>
        /// <exception cref="UnexpectedNumberOfRowsAffected">
        /// If the number of affected rows is greater than N
        /// </exception>
        public void WithoutReturnAffectingNRowsOrLess(string sql, int n, params object[] parameters)
        {
            WithoutReturnAffectingNRows(sql, n, true, parameters);
        }

        private void WithoutReturnAffectingNRows(string sql, int n, bool acceptsLess, params object[] parameters)
        {
            using (var connection = _connectionProvider.GetOpenConnection())
            using (var transaction = new TransactionScope())
            using (var command = connection.GetCommandWithParametersSet(sql, parameters.ToParameterDictionary()))
            {
                var affected = command.ExecuteNonQuery();
                if (affected == n || (acceptsLess && affected < n)) transaction.Complete();
                else throw new UnexpectedNumberOfRowsAffected(command, affected);
            }
        }
    }
}
