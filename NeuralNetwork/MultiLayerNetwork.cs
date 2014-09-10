using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace NeuralNetwork {
    public class MultiLayerNetwork {
        NetworkLayer[] Layers;
        int countLayers = 0, countX, countY;
        double[][] NETOUT;  // NETOUT[countLayers + 1][]
        double[][] DELTA;   // NETOUT[countLayers    ][]
        
        /* Создает полносвязанную сеть из n слоев. 
           sizeX - размерность вектора входных параметров
           layers - массив слоев. Значение элементов массива - количество нейронов в слое               
         */
        public MultiLayerNetwork(int sizeX, params int[] layers) {
            countLayers = layers.Length;
            countX = sizeX;
            countY = layers[layers.Length - 1];
            // Размерность выходов нейронов и Дельты
            NETOUT = new double[countLayers + 1][];
            NETOUT[0] = new double[sizeX];
            DELTA = new double[countLayers][];

            this.Layers = new NetworkLayer[countLayers];

            int countY1, countX1 = sizeX;
            // Устанавливаем размерность слоям и заполняем слоя случайными числами
            for (int i = 0; i < countLayers; i++) {
                countY1 = layers[i];

                NETOUT[i + 1] = new double[countY1];
                DELTA[i] = new double[countY1];

                this.Layers[i] = new NetworkLayer(countX1, countY1);
                this.Layers[i].GenerateWeights();
                countX1 = countY1;
            }
        }

        // Возвращает значение j-го слоя НС
        public void NetOUT(double[] inX, out double[] outY, int jLayer) {
            GetOUT(inX, jLayer);
            int N = NETOUT[jLayer].Length;

            outY = new double[N];

            for (int i = 0; i < N; i++) {
                outY[i] = NETOUT[jLayer][i];
            }

        }

        // Возвращает значение НС
        public void NetOUT(double[] inX, out double[] outY) {
            int j = countLayers;
            NetOUT(inX, out outY, j);
        }

        // Возвращает ошибку (метод наименьших квадратов)
        public double CalcError(double[] Y) {
            double kErr = 0;
            for (int i = 0; i < Y.Length; i++) {
                kErr += Math.Pow(Y[i] - NETOUT[countLayers][i], 2);
            }

            return 0.5 * kErr;
        }

        /* Обучает сеть, изменяя ее весовые коэффициэнты. 
           X, Y - обучающая пара. kLern - скорость обучаемости */
        public void LearnNW(double[] X, double[] Y, double kLearn) {
            // Вычисляем выход сети
            GetOUT(X);

            // Заполняем дельта последнего слоя
            for (int j = 0; j < Layers[countLayers - 1].countY; j++) {
                double O = NETOUT[countLayers][j];
                DELTA[countLayers - 1][j] = (Y[j] - O) * O * (1 - O);
            }

            // Перебираем все слои начиная споследнего 
            // изменяя веса и вычисляя дельта для скрытого слоя
            for (int k = countLayers - 1; k >= 0; k--) {
                // Изменяем веса выходного слоя
                for (int j = 0; j < Layers[k].countY; j++) {
                    for (int i = 0; i < Layers[k].countX; i++) {
                        Layers[k][i, j] += kLearn * DELTA[k][j] * NETOUT[k][i];
                    }
                }
                if (k > 0) {

                    // Вычисляем дельта слоя к-1
                    for (int j = 0; j < Layers[k - 1].countY; j++) {

                        double s = 0;
                        for (int i = 0; i < Layers[k].countY; i++) {
                            s += Layers[k][j, i] * DELTA[k][i];
                        }

                        DELTA[k - 1][j] = NETOUT[k][j] * (1 - NETOUT[k][j]) * s;
                    }
                }
            }
        }

        // Возвращает все значения нейронов до lastLayer слоя
        void GetOUT(double[] inX, int lastLayer) {
            double s;

            for (int j = 0; j < Layers[0].countX; j++)
                NETOUT[0][j] = inX[j];

            for (int i = 0; i < lastLayer; i++) {
                // размерность столбца проходящего через i-й слой
                for (int j = 0; j < Layers[i].countY; j++) {
                    s = 0;
                    for (int k = 0; k < Layers[i].countX; k++) {
                        s += Layers[i][k, j] * NETOUT[i][k];
                    }

                    // Вычисляем значение активационной функции
                    s = 1.0 / (1 + Math.Exp(-s));
                    NETOUT[i + 1][j] = 0.998 * s + 0.001;

                }
            }

        }

        // Возвращает все значения нейронов всех слоев
        void GetOUT(double[] inX) {
            GetOUT(inX, countLayers);
        }

        // Возвращает размер НС в байтах
        int GetSizeNW() {
            int sizeNW = sizeof(int) * (countLayers + 2);
            for (int i = 0; i < countLayers; i++) {
                sizeNW += sizeof(double) * Layers[i].countX * Layers[i].countY;
            }
            return sizeNW;
        }

        // Открывает НС
        public MultiLayerNetwork(String FileName) {
            OpenNW(FileName);
        }

        public MultiLayerNetwork(Stream inputStream) {
            List<byte> bytes = new List<byte>();
            byte[] buffer = new byte[1024];
            
            int c = 0;
            while ((c = inputStream.Read(buffer, 0, 1024)) > 0) {
                for (int q = 0; q < c; q++) {
                    bytes.Add(buffer[q]);
                }
            }

            OpenNW(bytes.ToArray());
            inputStream.Close();
        }

        public void OpenNW(String FileName) {
            byte[] binNW = File.ReadAllBytes(FileName);
            OpenNW(binNW);
        }

        public void OpenNW(byte[] binNW) {
            int k = 0;
            // Извлекаем количество слоев из массива
            countLayers = ReadFromArrayInt(binNW, ref k);
            Layers = new NetworkLayer[countLayers];

            // Извлекаем размерность слоев
            int CountY1 = 0, CountX1 = ReadFromArrayInt(binNW, ref k);
            // Размерность входа
            countX = CountX1;
            // Выделяемпамять под выходы нейронов и дельта
            NETOUT = new double[countLayers + 1][];
            NETOUT[0] = new double[CountX1];
            DELTA = new double[countLayers][];

            for (int i = 0; i < countLayers; i++) {
                CountY1 = ReadFromArrayInt(binNW, ref k);
                Layers[i] = new NetworkLayer(CountX1, CountY1);
                CountX1 = CountY1;

                // Выделяем память
                NETOUT[i + 1] = new double[CountY1];
                DELTA[i] = new double[CountY1];
            }
            // Размерность выхода
            countY = CountY1;
            // Извлекаем и записываем сами веса
            for (int r = 0; r < countLayers; r++)
                for (int p = 0; p < Layers[r].countX; p++)
                    for (int q = 0; q < Layers[r].countY; q++) {
                        Layers[r][p, q] = ReadFromArrayDouble(binNW, ref k);
                    }
        }

        // Сохраняет НС
        public void SaveNW(String FileName) {
            // размер сети в байтах
            int sizeNW = GetSizeNW();
            byte[] binNW = new byte[sizeNW];

            int k = 0;
            // Записываем размерности слоев в массив байтов
            WriteInArray(binNW, ref k, countLayers);
            if (countLayers <= 0)
                return;

            WriteInArray(binNW, ref k, Layers[0].countX);
            for (int i = 0; i < countLayers; i++)
                WriteInArray(binNW, ref k, Layers[i].countY);

            // Зпаисвыаем сами веса
            for (int r = 0; r < countLayers; r++)
                for (int p = 0; p < Layers[r].countX; p++)
                    for (int q = 0; q < Layers[r].countY; q++) {
                        WriteInArray(binNW, ref k, Layers[r][p, q]);
                    }


            File.WriteAllBytes(FileName, binNW);
        }

        // Разбивает переменную типа int на байты и записывает в массив
        void WriteInArray(byte[] mas, ref int pos, int value) {
            DataToByte DTB = new DataToByte();
            DTB.vInt = value;
            mas[pos++] = DTB.b1;
            mas[pos++] = DTB.b2;
            mas[pos++] = DTB.b3;
            mas[pos++] = DTB.b4;
        }

        // Разбивает переменную типа int на байты и записывает в массив
        void WriteInArray(byte[] mas, ref int pos, double value) {
            DataToByte DTB = new DataToByte();
            DTB.vDouble = value;
            mas[pos++] = DTB.b1;
            mas[pos++] = DTB.b2;
            mas[pos++] = DTB.b3;
            mas[pos++] = DTB.b4;
            mas[pos++] = DTB.b5;
            mas[pos++] = DTB.b6;
            mas[pos++] = DTB.b7;
            mas[pos++] = DTB.b8;
        }

        // Извлекает переменную типа int из 4-х байтов массива
        int ReadFromArrayInt(byte[] mas, ref int pos) {
            DataToByte DTB = new DataToByte();
            DTB.b1 = mas[pos++];
            DTB.b2 = mas[pos++];
            DTB.b3 = mas[pos++];
            DTB.b4 = mas[pos++];

            return DTB.vInt;
        }

        // Извлекает переменную типа double из 8-ми байтов массива
        double ReadFromArrayDouble(byte[] mas, ref int pos) {
            DataToByte DTB = new DataToByte();
            DTB.b1 = mas[pos++];
            DTB.b2 = mas[pos++];
            DTB.b3 = mas[pos++];
            DTB.b4 = mas[pos++];
            DTB.b5 = mas[pos++];
            DTB.b6 = mas[pos++];
            DTB.b7 = mas[pos++];
            DTB.b8 = mas[pos++];

            return DTB.vDouble;
        }

        // Структура дря разбиения переменных типа int и double на байты
        [StructLayout(LayoutKind.Explicit)]
        internal class DataToByte {
            [FieldOffset(0)]
            public double vDouble;

            [FieldOffset(0)]
            public int vInt;

            [FieldOffset(0)]
            public byte b1;
            [FieldOffset(1)]
            public byte b2;
            [FieldOffset(2)]
            public byte b3;
            [FieldOffset(3)]
            public byte b4;
            [FieldOffset(4)]
            public byte b5;
            [FieldOffset(5)]
            public byte b6;
            [FieldOffset(6)]
            public byte b7;
            [FieldOffset(7)]
            public byte b8;
        }
    }
}
