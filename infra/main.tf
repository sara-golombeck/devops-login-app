module "vpc" {
  source = "./modules/network"

  vpc_name                = var.vpc_name
  vpc_cidr                = var.vpc_cidr
  availability_zones      = var.availability_zones
  public_subnet_cidrs     = var.public_subnet_cidrs
  private_subnet_cidrs    = var.private_subnet_cidrs
  cluster_name            = var.cluster_name
  common_tags             = var.common_tags
}

module "eks" {
  source = "./modules/eks"

  cluster_name        = var.cluster_name
  cluster_version     = var.cluster_version
  vpc_id              = module.vpc.vpc_id
  
  private_subnet      = module.vpc.private_subnet
  private_subnet_cidrs = module.vpc.private_subnet_cidrs
  
  public_access_cidrs = var.public_access_cidrs
  admin_user_arn      = var.admin_user_arn
  
  node_group_config   = var.node_group_config
  cluster_addons      = var.cluster_addons
  common_tags         = var.common_tags

  depends_on = [module.vpc]
}

module "argocd" {
  source = "./modules/argocd"
  gitops_repo_url = "https://github.com/sara-golombeck/gitops-email-service.git"
  

  eks_cluster_name        = var.cluster_name
  aws_region              = var.aws_region
  argocd_namespace        = var.argocd_namespace
  infra_path              = "argocd"
  
  depends_on = [module.eks]
}

module "sqs" {
  source = "./modules/sqs"
  
  project_name = var.project_name
  common_tags  = var.common_tags
}

module "ses" {
  source = "./modules/ses"
  
  domain_name = var.domain_name
  common_tags = var.common_tags
}

module "s3_static" {
  source = "./modules/s3-static"
  
  project_name      = var.project_name
  aws_region        = var.aws_region
  common_tags       = var.common_tags
  
  use_custom_domain = true
  domain_name       = var.addit_domain_name
  zone_id          = module.route53_zone.zone_id
}


module "route53_zone" {
  source      = "./modules/route53-zone"
  domain_name = var.addit_domain_name
}


resource "aws_route53_record" "root" {
  zone_id = module.route53_zone.zone_id
  name    = var.addit_domain_name
  type    = "A"
  
  alias {
    name                   = module.s3_static.cloudfront_domain_name
    zone_id               = module.s3_static.cloudfront_zone_id
    evaluate_target_health = false
  }
}

module "iam" {
  source = "./modules/iam"
  
  project_name              = var.project_name
  cluster_oidc_issuer_arn   = module.eks.oidc_provider_arn
  cluster_oidc_issuer_url   = module.eks.cluster_oidc_issuer_url
  namespace                 = var.namespace
  sqs_queue_arn            = module.sqs.queue_arn
  ses_domain_arn           = module.ses.domain_identity_arn
  common_tags              = var.common_tags
  
  depends_on = [module.eks, module.sqs, module.ses]
}


