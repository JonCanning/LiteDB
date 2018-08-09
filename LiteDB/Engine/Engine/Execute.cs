﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LiteDB.Engine
{
    public partial class LiteEngine
    {
        /// <summary>
        /// Execute single SQL-Like command and return data reader (can contains single or multiple resultsets). 
        /// Will execute only first command. Need NextResult() called to run anothers commands
        /// </summary>
        public BsonDataReader Execute(string command, BsonDocument parameters)
        {
            var tokenizer = new Tokenizer(command);
            var sql = new SqlParser(this, tokenizer, parameters);
            var reader = sql.Execute();

            // when request .NextResult() run another SqlParser
            reader.FetchNextResult += () =>
            {
                // checks if has more tokens
                if (tokenizer.Current.Type == TokenType.EOF) return null;

                if (tokenizer.Current.Type == TokenType.SemiColon)
                {
                    var ahead = tokenizer.LookAhead();

                    if (ahead.Type == TokenType.EOF) return null;
                }

                var next = new SqlParser(this, tokenizer, parameters);

                return next.Execute();
            };

            return reader;
        }
    }
}