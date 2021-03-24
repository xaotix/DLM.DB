﻿using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DB
{
    public class Tabela
    {
        public override string ToString()
        {
            return "[" + Nome + "]" + "/L:" + Linhas.Count();
        }
        public string Nome { get; set; } = "";

        public Tabela Carregar(string Arquivo)
        {

            if (File.Exists(Arquivo))
            {
                try
                {
                    using (ZipFile zip = ZipFile.Read(Arquivo))
                    {
                        ZipEntry e = zip["tabela.dbdlm"];
                        if (zip.Entries.Count > 0)
                        {

                            XmlSerializer x = new XmlSerializer(typeof(Tabela));
                            Tabela ts = (Tabela)x.Deserialize(zip.Entries.ToArray()[0].OpenReader());
                            this.Linhas = ts.Linhas;
                            this.Nome = ts.Nome;
                            return ts;

                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro descompactando arquivo .dbdlm\n" + ex.Message + "\n" + ex.StackTrace);
                }
            }
            return new Tabela();
        }

        public bool Gravar(string Arquivo)
        {
            try
            {

                string arquivo_tmp = Application.StartupPath + @"\tabela.dbdlm";

                if (File.Exists(arquivo_tmp))
                {
                    File.Delete(arquivo_tmp);
                }

                XmlSerializer x = new XmlSerializer(typeof(Tabela));
                TextWriter writer = new StreamWriter(arquivo_tmp);
                x.Serialize(writer, this);
                writer.Close();

                if (File.Exists(Arquivo))
                {
                    File.Delete(Arquivo);
                }
                using (ZipFile zip = new ZipFile())
                {

                    zip.AddFile(arquivo_tmp, "");

                    zip.Save(Arquivo);
                }
                File.Delete(arquivo_tmp);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro Tentando criar o arquivo {Arquivo}\n" + ex.Message + "\n" + ex.StackTrace);
                return false;
            }

        }


        public List<string> GetColunas()
        {
            return Linhas.SelectMany(x => x.Header).Distinct().ToList();
        }

        public List<Linha> Linhas { get; set; } = new List<Linha>();

        public List<Linha> Filtrar  (string Chave, string Valor, bool exato =false)
        {
            List<Linha> Retorno = new List<Linha>();
            if(exato)
            {
                return Linhas.FindAll(x => x.Celulas.FindAll(y => y.Coluna == Chave && y.Valor == Valor).Count>0);
            }
            else
            {
                return Linhas.FindAll(x => x.Celulas.FindAll(y => y.Coluna.ToLower().Replace(" ","") == Chave.ToLower().Replace(" ", "") && y.Valor.ToLower().Replace(" ", "").Contains(Valor.ToLower().Replace(" ", ""))).Count > 0);
            }          
        }

        public Tabela(List<Linha> Linhas, string Nome)
        {
            this.Nome = Nome;
            this.Linhas = Linhas;
        }

        public Tabela(string Tabela)
        {
            this.Nome = Tabela;
        }

        public Tabela Filtro(string Chave,string Valor,bool Exato)
        {
            return new Tabela(Filtrar(Chave, Valor, Exato),Nome);
        }

        public Tabela()
        {

        }
    }

}