﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRedisDocuments
{
    public interface IDeletable
    {
        Task DeleteAsync();
    }
}
