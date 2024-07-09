using BenchmarkDotNet.Attributes;
using Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
    public class Entity
    {
        public Vector3 position;
        public int id;
        public string name;
    }

    public class MyBenchmark
    {
        List<Entity> data = new List<Entity>();
        KDTree3Naive<int> tree3Int = new KDTree3Naive<int>();
        KDTree3I2<Entity> tree3Struct = new KDTree3I2<Entity>();
        KDTree3Naive<Entity> tree3Naive = new KDTree3Naive<Entity>();
        KDTree3I<Entity> tree3Indexed = new KDTree3I<Entity>();

        public MyBenchmark()
        {
            Random random = new Random();
            for (int i = 0; i < 5000; i++)
            {
                data.Add(new Entity()
                {
                    position = new Vector3((float)random.NextDouble() * 200, 0, (float)random.NextDouble() * 200),
                    id = i,
                });
            }
        }

        [Benchmark]
        public void Tree3Naive()
        {
            foreach (var entity in data)
            {
                tree3Naive.Add(entity, entity.position);
            }
            tree3Naive.Build();

            for (int i = 0; i < 200; i++)
            {
                for (int j = 0; j < 100; j++)
                    tree3Naive.ForRange(new Vector3(i, 0, j), 10, SearchCallBack);
            }

            tree3Naive.Clear();
        }

        [Benchmark]
        public void Tree3Indexed()
        {
            foreach (var entity in data)
            {
                tree3Indexed.Add(entity, entity.position);
            }
            tree3Indexed.Build();
            for (int i = 0; i < 200; i++)
            {
                for (int j = 0; j < 100; j++)
                    tree3Indexed.ForRange(new Vector3(i, 0, j), 10, SearchCallBack);
            }

            tree3Indexed.Clear();
        }

        [Benchmark]
        public void Tree3IndexedParallel()
        {
            foreach (var entity in data)
            {
                tree3Indexed.Add(entity, entity.position);
            }
            tree3Indexed.ParallelBuild();
            for (int i = 0; i < 200; i++)
            {
                for (int j = 0; j < 100; j++)
                    tree3Indexed.ForRange(new Vector3(i, 0, j), 10, SearchCallBack);
            }

            tree3Indexed.Clear();
        }

        //[Benchmark]
        //public void Tree3IntStruct()
        //{
        //    foreach (var entity in data)
        //    {
        //        tree3Struct.Add(entity, entity.position);
        //    }
        //    tree3Struct.Build();
        //    for (int i = 0; i < 200; i++)
        //    {
        //        for (int j = 0; j < 100; j++)
        //            tree3Struct.ForRange(new Vector3(i, 0, j), 10, SearchCallBack);
        //    }

        //    tree3Struct.Clear();
        //}

        void SearchCallBack(Entity entity)
        {

        }
    }
}
