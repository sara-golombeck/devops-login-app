terraform {
 required_providers {
   kubectl = {
     source  = "gavinbunney/kubectl"
     version = "~> 1.14.0"
   }
 }
}


resource "helm_release" "argocd" {
 name             = var.argocd_namespace
 namespace        = var.argocd_namespace
 create_namespace = true
 repository       = "https://argoproj.github.io/argo-helm"
 chart            = "argo-cd"
 version          = "8.0.10"
 
 values = [
   yamlencode({
     server = {
       service = {
         type = "LoadBalancer"
         annotations = {
           "service.beta.kubernetes.io/aws-load-balancer-source-ranges" = "212.76.117.214/32"
         }
       }
     }
   })
 ]
}


data "aws_secretsmanager_secret" "github_token" {
 name = "github-token"
}

data "aws_secretsmanager_secret_version" "github_token" {
 secret_id = data.aws_secretsmanager_secret.github_token.id
}


resource "kubernetes_secret" "argocd_repo_https" {
 metadata {
   name      = "github-https-creds"
   namespace = var.argocd_namespace
   labels = {
     "argocd.argoproj.io/secret-type" = "repo-creds"
   }
 }
 
 data = {
   type     = "git"
   url      = "https://github.com/sara-golombeck"
   username = "sara-golombeck"
   password = jsondecode(data.aws_secretsmanager_secret_version.github_token.secret_string)["token"]
 }
 
 type = "Opaque"
 depends_on = [helm_release.argocd]
}


resource "kubectl_manifest" "argocd_root_application" {
 yaml_body = templatefile("${path.module}/templates/root_app.yaml", {
   name            = "root-app"
   namespace       = var.argocd_namespace
   repo_url        = var.gitops_repo_url
   infra_namespace = var.argocd_namespace
   infra_path      = var.infra_path
 })
 
 depends_on = [
   helm_release.argocd,
   kubernetes_secret.argocd_repo_https
 ]
}

