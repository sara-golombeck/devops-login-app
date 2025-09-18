
resource "aws_eks_cluster" "main" {
  name     = var.cluster_name
  role_arn = aws_iam_role.cluster.arn
  version  = var.cluster_version

  vpc_config {
    subnet_ids = var.private_subnet[*].id
    endpoint_private_access = true
    endpoint_public_access  = true
    public_access_cidrs    = var.public_access_cidrs
    security_group_ids = [aws_security_group.cluster.id]
  }

  # API Access Entry Configuration
  access_config {
    authentication_mode = "API"
    bootstrap_cluster_creator_admin_permissions = true
  }

  depends_on = [
    aws_iam_role_policy_attachment.cluster_AmazonEKSClusterPolicy,
  ]

  tags = var.common_tags
}


resource "aws_launch_template" "node_group" {
  name_prefix = "${var.cluster_name}-node-"
  
  vpc_security_group_ids = [aws_security_group.node_group.id]
  

  block_device_mappings {
    device_name = "/dev/xvda"
    
    ebs {
      volume_size = var.node_group_config.disk_size
      volume_type = "gp3"
      encrypted   = true
      delete_on_termination = true
    }
  }
  
  metadata_options {
    http_endpoint               = "enabled"
    http_tokens                 = "required"
    http_put_response_hop_limit = 2
    instance_metadata_tags      = "enabled"
  }
  
  tag_specifications {
    resource_type = "instance"
    
    tags = merge(var.common_tags, {
      Name = "${var.cluster_name}-node"
      "kubernetes.io/cluster/${var.cluster_name}" = "owned"
    })
  }
  
  tag_specifications {
    resource_type = "volume"
    
    tags = merge(var.common_tags, {
      Name = "${var.cluster_name}-node-volume"
      "kubernetes.io/cluster/${var.cluster_name}" = "owned"
    })
  }
  
  tag_specifications {
    resource_type = "network-interface"
    
    tags = merge(var.common_tags, {
      Name = "${var.cluster_name}-node-eni"
      "kubernetes.io/cluster/${var.cluster_name}" = "owned"
    })
  }
  
  tags = var.common_tags
}

resource "aws_eks_node_group" "main" {
  cluster_name    = aws_eks_cluster.main.name
  node_group_name = "${var.cluster_name}-nodes"
  node_role_arn   = aws_iam_role.node_group.arn
  
  subnet_ids = var.private_subnet[*].id
  capacity_type  = var.node_group_config.capacity_type
  instance_types = var.node_group_config.instance_types
  
  launch_template {
    id      = aws_launch_template.node_group.id
    version = "$Latest"
  }



  scaling_config {
    desired_size = var.node_group_config.desired_size
    max_size     = var.node_group_config.max_size
    min_size     = var.node_group_config.min_size
  }

  update_config {
    max_unavailable = 1
  }

  tags = merge(var.common_tags, {
    Name = "${var.cluster_name}-node-group"
    "kubernetes.io/cluster/${var.cluster_name}" = "owned"
  })

  depends_on = [
    aws_iam_role_policy_attachment.node_group_AmazonEKSWorkerNodePolicy,
    aws_iam_role_policy_attachment.node_group_AmazonEKS_CNI_Policy,
    aws_iam_role_policy_attachment.node_group_AmazonEC2ContainerRegistryReadOnly,
    aws_eks_cluster.main
  ]
  
  lifecycle {
    create_before_destroy = true
    ignore_changes        = [scaling_config[0].desired_size]
  }
}

resource "kubernetes_storage_class" "gp3_default" {
  metadata {
    name = "gp3"
    annotations = {
      "storageclass.kubernetes.io/is-default-class" = "true"
    }
  }
  
  storage_provisioner    = "ebs.csi.aws.com"
  volume_binding_mode    = "WaitForFirstConsumer"
  allow_volume_expansion = true
  reclaim_policy        = "Delete"
  
  parameters = {
    type      = "gp3"
    fsType    = "ext4"
    encrypted = "true"
  }
  
  depends_on = [
    aws_eks_node_group.main,
    aws_eks_addon.ebs_csi_driver
  ]
}