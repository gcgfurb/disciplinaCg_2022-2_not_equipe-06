﻿#define CG_Gizmo
// #define CG_Privado

using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using CG_Biblioteca;
using OpenTK.Input;
using System;
using OpenTK;
using System.Linq;

namespace gcgcg
{
    class Mundo : GameWindow
    {
        private static Mundo instanciaMundo = null;

        private Mundo(int width, int height) : base(width, height) { }

        public static Mundo GetInstance(int width, int height)
        {
            if (instanciaMundo == null)
                instanciaMundo = new Mundo(width, height);
            return instanciaMundo;
        }

        private CameraPerspective camera = new CameraPerspective();
        protected List<Objeto> objetosLista = new List<Objeto>();
        private ObjetoGeometria objetoSelecionado = null;
        private char objetoId = '@';
        private String menuSelecao = "";
        private char menuEixoSelecao = 'z';
        private float deslocamento = 0;
        private bool bBoxDesenhar = false;
        private bool showGizmo = false;

        //  Character Controler
        private BBox bBox;
        private bool firstPerson = true;
        private Vector3 cameraPositionAt;
        private Vector3 cameraPositionEye;
        private Character character;
        private Crown crown;
        private bool victory = false;
        //  Character Controler

        //  Controle de movimentação
        private bool front = false;
        private bool back = true;
        private bool left = false;
        private bool right = false;
        private bool coroaAdicionada = false;
        private Cubo start;
        private Cubo end;
        private readonly Random random = new Random();
        //  Controle de movimentação

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Console.WriteLine(" --- Ajuda / Teclas: ");
            Console.WriteLine(" [  H     ] mostra teclas usadas. ");

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            // GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.Light0);

            GL.Light(LightName.Light0, LightParameter.Position, new float[] { 0.0f, 2.0f, 0.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.3f, 0.3f, 0.3f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.SpotExponent, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.2f, 0.2f, 0.2f, 1.0f });
            GL.LightModel(LightModelParameter.LightModelTwoSide, 1);
            GL.LightModel(LightModelParameter.LightModelLocalViewer, 1);

            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, new float[] { 0.3f, 0.3f, 0.3f, 1.0f });
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Material(MaterialFace.Front, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
            GL.Material(MaterialFace.Front, MaterialParameter.ColorIndexes, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, 0.001f);

            // Mapa do jogo
            objetoId = Utilitario.charProximo(objetoId);
            start = new Cubo(objetoId, null);
            start.ObjetoCor.CorR = 0; start.ObjetoCor.CorG = 0; start.ObjetoCor.CorB = 0;
            objetosLista.Add(start);
            objetoSelecionado = start;
            objetoSelecionado.EscalaXYZBBox(10, 1, 1);
            objetoSelecionado.Translacao(45, 'x');
            objetoSelecionado.Translacao(-10, 'z');

            objetoSelecionado.ObjetoCor.CorR = 27;
            objetoSelecionado.ObjetoCor.CorG = 36;
            objetoSelecionado.ObjetoCor.CorB = 158;

            GerarCaminho();

            objetoId = Utilitario.charProximo(objetoId);
            end = new Cubo(objetoId, null);
            objetosLista.Add(end);
            objetoSelecionado = end;
            objetoSelecionado.EscalaXYZBBox(10, 1, 1);
            objetoSelecionado.Translacao(45, 'x');
            objetoSelecionado.Translacao(100, 'z');

            objetoSelecionado.ObjetoCor.CorR = 44;
            objetoSelecionado.ObjetoCor.CorG = 163;
            objetoSelecionado.ObjetoCor.CorB = 31;
            // Mapa do jogo

            // Personagem
            objetoId = Utilitario.charProximo(objetoId);
            character = new Character(objetoId, null);
            objetosLista.Add(character);

            objetoSelecionado = character;

            objetoSelecionado.Translacao(-5, 'z');
            objetoSelecionado.Translacao(33.66f, 'x');
            objetoSelecionado.Escala(1.5f);
            objetoSelecionado.Rotacao(180, 'y');

            updadeCharacterBBox();
            // Personagem
        }

        private List<int[]> GerarCaminhoAleatorio(int sizeX, int sizeY, int startX, int endX)
        {
            int x = startX;
            List<int[]> path = new List<int[]>();


            for (int y = sizeY - 1; y >= 0; y--)
            {
                int upX = y > 0 ? random.Next(0, sizeX) : endX;

                while (x != upX)
                {
                    path.Add(new int[] { x, y });

                    if (x < upX)
                    {
                        x++;
                    }
                    else
                    {
                        x--;
                    }
                }

                path.Add(new int[] { x, y });
            }

            return path;
        }

        private void GerarCaminho()
        {
            var plataformas = objetosLista.OfType<Plataforma>().ToList();

            foreach(var plataforma in plataformas)
            {
                objetosLista.Remove(plataforma);
            }

            const int largura = 10;
            const int altura = 10;
            const int caminhos = 3;

            int[,] caminho = new int[largura, altura] {
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            };

            for(int i = 0; i < caminhos; i++)
            {
                List<int[]> caminhoAleatorio = GerarCaminhoAleatorio(
                    largura, altura,
                    random.Next(0, largura - 1),
                    random.Next(0, largura - 1)
                );

                foreach(int[] posicao in caminhoAleatorio)
                {
                    caminho[posicao[0], posicao[1]] = 1;
                }
            }

            for (int col = 0; col < 10; col++)
            {
                for (int row = 0; row < 10; row++)
                {
                    objetoId = Utilitario.charProximo(objetoId);
                    Plataforma item = new Plataforma(objetoId, null);
                    item.isFalso = caminho[col, row] == 0 ? true : false;
                    objetosLista.Add(item);
                    item.Translacao(row * 10, 'x');
                    item.Translacao(col * 10, 'z');

                    if(item.isFalso)
                    {
                        item.ObjetoCor.CorR = 255;
                        item.ObjetoCor.CorG = 0;
                        item.ObjetoCor.CorB = 0;
                    } else
                    {
                        item.ObjetoCor.CorR = 0;
                        item.ObjetoCor.CorG = 255;
                        item.ObjetoCor.CorB = 0;
                    }

                    //item.ObjetoCor.CorR = 70;
                    //item.ObjetoCor.CorG = 25;
                    //item.ObjetoCor.CorB = 110;
                }
            }
        }

        public void resetCharacter()
        {
            objetoSelecionado.Matriz.AtribuirIdentidade();
            objetoSelecionado.Translacao(-5, 'z');
            objetoSelecionado.Translacao(33.66f, 'x');
            objetoSelecionado.Escala(1.5f);
            objetoSelecionado.Rotacao(180, 'y');

            front = false;
            back = true;
            left = false;
            right = false;

            victory = false;
            removeCrown();
            GerarCaminho();
        }

        public void addCrown()
        {
            if(coroaAdicionada)
            {
                return;
            }

            coroaAdicionada = true;

            objetoId = Utilitario.charProximo(objetoId);
            crown = new Crown(objetoId, null);
            objetoSelecionado.FilhoAdicionar(crown);
        }

        public void removeCrown() // TODO - FIX Remover a coroa do personagem
        {
            coroaAdicionada = false;
            objetoSelecionado.FilhoRemover(crown);
        }

        public void updadeCharacterBBox()
        {
            bBox = new BBox(
                (float)objetoSelecionado.Matriz.ObterDados()[12] - 2.5f,
                (float)objetoSelecionado.Matriz.ObterDados()[13],
                (float)objetoSelecionado.Matriz.ObterDados()[14] - 2.5f,

                (float)objetoSelecionado.Matriz.ObterDados()[12] + 2.5f,
                (float)objetoSelecionado.Matriz.ObterDados()[13] + 6.0f,
                (float)objetoSelecionado.Matriz.ObterDados()[14] + 2.5f);
            bBox.ProcessarCentro();
            objetoSelecionado.BBox = bBox;
        }

        private Vector3 getCharacterLocation()
        {
            return new Vector3((float)objetoSelecionado.BBox.obterCentro.X,
                               (float)objetoSelecionado.BBox.obterCentro.Y,
                               (float)objetoSelecionado.BBox.obterCentro.Z);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(camera.Fovy, Width / (float)Height, camera.Near, 200.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            updadeCharacterBBox();

            if (objetoSelecionado.BBox.obterCentro.Z > 102) victory = true;

            if (victory) addCrown();

            if (firstPerson)
            {
                cameraPositionEye = new Vector3((float)objetoSelecionado.BBox.obterCentro.X, (float)objetoSelecionado.BBox.obterCentro.Y + 20, (float)objetoSelecionado.BBox.obterCentro.Z - 40);
                cameraPositionAt = new Vector3((float)objetoSelecionado.BBox.obterCentro.X, (float)objetoSelecionado.BBox.obterCentro.Y, (float)objetoSelecionado.BBox.obterCentro.Z + 20);
            }
            else
            {
                cameraPositionEye = new Vector3(50.5f, 30.0f, -65.0f);
                cameraPositionAt = new Vector3(50.5f, 0.0f, 20.0f);
            }

            Matrix4 modelview = Matrix4.LookAt(cameraPositionEye, cameraPositionAt, camera.Up);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

#if CG_Gizmo
            Sru3D();
#endif
            for (var i = 0; i < objetosLista.Count; i++)
                objetosLista[i].Desenhar();
            if (bBoxDesenhar && (objetoSelecionado != null))
            {
                objetoSelecionado.BBox.Desenhar();
            }

            this.SwapBuffers();
        }

        protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.H) Utilitario.AjudaTeclado();
            else if (e.Key == Key.Escape) Exit();
            //--------------------------------------------------------------
            else if (e.Key == Key.E)
            {
                Console.WriteLine("--- Objetos / Pontos: ");
                for (var i = 0; i < objetosLista.Count; i++)
                {
                    Console.WriteLine(objetosLista[i]);
                }
            }
            //--------------------------------------------------------------
            else if (e.Key == Key.X) menuEixoSelecao = 'x';
            else if (e.Key == Key.Y) menuEixoSelecao = 'y';
            else if (e.Key == Key.Z) menuEixoSelecao = 'z';
            else if (e.Key == Key.Minus) deslocamento--;
            else if (e.Key == Key.Plus) deslocamento++;

            // Movimentação do Personagem 
            else if (e.Key == Key.W) // Para frente
            {
                front = true;
                if (left)
                {
                    character.Rotacao(-90, 'y');
                    left = false;
                }
                else if (right)
                {
                    character.Rotacao(90, 'y');
                    right = false;
                }
                else if (front && back) objetoSelecionado.Rotacao(180, 'y');
                objetoSelecionado.Translacao(1, 'z');
                back = false;
                left = false;
                right = false;
            }
            else if (e.Key == Key.S) // Para trás
            {
                back = true;
                if (left)
                {
                    character.Rotacao(90, 'y');
                    left = false;
                }
                else if (right)
                {
                    character.Rotacao(-90, 'y');
                    right = false;
                }
                if (back && front) objetoSelecionado.Rotacao(180, 'y');
                objetoSelecionado.Translacao(-1, 'z');
                front = false;
                left = false;
                right = false;
            }
            else if (e.Key == Key.A) // Para esquerda
            {
                left = true;
                if (left && right) objetoSelecionado.Rotacao(180, 'y');
                else if (left && front)
                {
                    objetoSelecionado.Rotacao(90, 'y');
                    front = false;
                }
                else if (left && back)
                {
                    objetoSelecionado.Rotacao(-90, 'y');
                    back = false;
                }
                objetoSelecionado.Translacao(1, 'x');
                right = false;
                front = false;
                back = false;
            }
            else if (e.Key == Key.D) // Para direita
            {
                right = true;
                if (right && left) objetoSelecionado.Rotacao(-180, 'y');
                else if (right && front)
                {
                    objetoSelecionado.Rotacao(-90, 'y');
                    front = false;
                }
                else if (right && back)
                {
                    objetoSelecionado.Rotacao(90, 'y');
                    back = false;
                }
                objetoSelecionado.Translacao(-1, 'x');
                left = false;
                front = false;
                back = false;
            }
            // Movimentação do Personagem 

            else if (e.Key == Key.P)
            {
                Console.WriteLine("Location: " + getCharacterLocation());
            }

            // Altera modo de câmera
            else if (e.Key == Key.C) firstPerson = !firstPerson;
            // Altera modo de câmera

            else if (e.Key == Key.R) resetCharacter();

            else if (e.Key == Key.O) bBoxDesenhar = !bBoxDesenhar;

            else if (e.Key == Key.G) showGizmo = !showGizmo;

            else
                Console.WriteLine(" __ Tecla não implementada.");
        }

#if CG_Gizmo
        private void Sru3D()
        {
            if (showGizmo)
            {
                GL.LineWidth(1);
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Convert.ToByte(255), Convert.ToByte(0), Convert.ToByte(0));
                GL.Vertex3(0, 0, 0); GL.Vertex3(200, 0, 0);

                GL.Color3(Convert.ToByte(0), Convert.ToByte(255), Convert.ToByte(0));
                GL.Vertex3(0, 0, 0); GL.Vertex3(0, 200, 0);

                GL.Color3(Convert.ToByte(0), Convert.ToByte(0), Convert.ToByte(255));
                GL.Vertex3(0, 0, 0); GL.Vertex3(0, 0, 200);
                GL.End();
            }
        }
#endif
    }
    class Program
    {
        static void Main(string[] args)
        {
            ToolkitOptions.Default.EnableHighResolution = false;
            Mundo window = Mundo.GetInstance(600, 600);
            window.Title = "CG_N4: Fall Guy";
            window.Run(1.0 / 60.0);
        }
    }
}
