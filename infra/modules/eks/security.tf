# Security Groups for EKS Cluster and Node Group

# Security Group for EKS Cluster Control Plane
resource "aws_security_group" "cluster" {
  name_prefix = "${var.cluster_name}-cluster-sg"
  vpc_id      = var.vpc_id
  description = "Security group for EKS cluster control plane"

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(var.common_tags, {
    Name = "${var.cluster_name}-cluster-sg"
  })
}

# Security Group for EKS Node Group
resource "aws_security_group" "node_group" {
  name_prefix = "${var.cluster_name}-node-group-sg"
  vpc_id      = var.vpc_id
  description = "Security group for EKS node group"

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(var.common_tags, {
    Name = "${var.cluster_name}-node-group-sg"
  })
}

# Allow nodes to communicate with cluster API
resource "aws_security_group_rule" "node_to_cluster" {
  type                     = "ingress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.node_group.id
  security_group_id        = aws_security_group.cluster.id
  description              = "Allow pods to communicate with the cluster API"
}

# Allow cluster to communicate with node kubelet
resource "aws_security_group_rule" "cluster_to_node_kubelet" {
  type                     = "ingress"
  from_port                = 10250
  to_port                  = 10250
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.cluster.id
  security_group_id        = aws_security_group.node_group.id
  description              = "Allow cluster to communicate with worker node kubelet"
}

# Allow cluster to communicate with nodes (1025-65535)
resource "aws_security_group_rule" "cluster_to_node" {
  type                     = "ingress"
  from_port                = 1025
  to_port                  = 65535
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.cluster.id
  security_group_id        = aws_security_group.node_group.id
  description              = "Allow cluster to communicate with worker node container runtime"
}

# Allow nodes to communicate with each other
resource "aws_security_group_rule" "node_to_node" {
  type                     = "ingress"
  from_port                = 0
  to_port                  = 65535
  protocol                 = "-1"
  source_security_group_id = aws_security_group.node_group.id
  security_group_id        = aws_security_group.node_group.id
  description              = "Allow nodes to communicate with each other"
}

# Allow pods to communicate with cluster DNS
resource "aws_security_group_rule" "node_dns" {
  type              = "ingress"
  from_port         = 53
  to_port           = 53
  protocol          = "udp"
  source_security_group_id = aws_security_group.node_group.id
  security_group_id = aws_security_group.node_group.id
  description       = "Allow pods to communicate with cluster DNS"
}