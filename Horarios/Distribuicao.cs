﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILOG.Concert;
using ILOG.CPLEX;
using Plotter;

namespace ProjetoPO
{
    class Distribuicao
    {
        public static void Main(string[] args)
        {

            //*******************************************************
            //  Obtem os dados de arquivo externo
            //
            //*******************************************************

            var files = Directory.GetFiles(@"./distribuicao/");

            foreach (var file in files)
            {
                if (Path.GetFileNameWithoutExtension(file) != "RC101")
                    continue;

                Solve(file);
            }

            Console.WriteLine("Press any key to close the program...");
            Console.ReadKey(true);

        }

        private static void Solve(string filePath)
        {
            var points = ReadPoints(filePath);

            var matrix = AssembleMatrix(points);

            var model = new Cplex();

            //
            // Create decision variable Y
            var X = new MatrizAdjacenciaSimetrica<INumVar>(matrix.N);

            for (int i = 0; i < matrix.N; i++)
            {
                for (int j = i; j < matrix.N; j++)
                {
                    X[i, j] = model.BoolVar();
                }

            }

            //
            // Create decision variable Y
            var Y = new MatrizAdjacenciaSimetrica<INumVar>(matrix.N);

            for (int i = 0; i < matrix.N; i++)
            {
                for (int j = i; j < matrix.N; j++)
                {
                    X[i, j] = model.BoolVar();
                }

            }

            //----------------------------------------------------------------------------
            // RESTRICTIONS
            //----------------------------------------------------------------------------


            // Forces every vertex execept the zeroth to connect to another one.    
            for (int i = 1; i < X.N; i++)
            {
                var exp = model.LinearNumExpr();

                for (int j = 0; j < X.N; j++)
                {
                    if (i != j)
                    {
                        exp.AddTerm(1.0, X[i, j]);
                    }
                }

                // Each vertex must connect to another one.
                model.AddEq(exp, 2.0);
            }

            // Forces that the depot connect to everyone (not including himself)
            {
                var exp = model.LinearNumExpr();

                for (int j = 1; j < X.N; j++)
                {
                    if (j != 0)
                    {
                        exp.AddTerm(1.0, X[0, j]);
                    }
                }

                // Each vertex must connect to another one.
                model.AddEq(exp, matrix.N - 1);
            }


            throw new NotImplementedException();
        }

        public List<PointD> ReadFile(string filePath)
        {
            int vehicleNumber, capacity;

            var lines = File.ReadAllLines(filePath);

            var @params = lines[4].Split('\t');

            vehicleNumber = int.Parse(@params[0]);
            capacity = int.Parse(@params[1]);
        }

        static List<PointD> ReadPoints(string filePath)
        {

            var lines = File.ReadAllLines(filePath);
            var chartPoints = new List<PointD>(int.Parse(lines[0]));


            for (int i = 1; i < lines.Length; i++)
            {
                //TODO Implementar a leitura

                var X = 0;
                var Y = 0;
                var point = new PointD { X = X, Y = Y };
                chartPoints.Add(point);
            }

            return chartPoints;

        }

        private static MatrizAdjacenciaSimetrica<double> AssembleMatrix(List<PointD> points)
        {
            var matrix = new MatrizAdjacenciaSimetrica<double>(points.Count);

            for (int i = 0; i < matrix.N - 1; i++)
            {
                for (int j = i + 1; j < matrix.N; j++)
                {
                    matrix.Set(i, j, Distance(points[i], points[j]));
                }
            }

            return matrix;
        }

        // Distancia euclidiana entre 2 pontos.
        private static double Distance(PointD p1, PointD p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}
